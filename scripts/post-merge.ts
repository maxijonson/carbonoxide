import { readFileSync, writeFileSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));

interface TypeBlock {
  blockKind: "type";
  kind: "class" | "enum" | "interface" | "struct" | "record";
  isPartial: boolean;
  name: string; // e.g. "PluginConfig",
  modifiers: string; // e.g. "public abstract"
  baseList: string[]; // e.g. ["IEnumerable", "ICollection"]
  attributes: string[]; // full attribute lines (untrimmed, preserving indent)
  constraint: string | null; // e.g. "where T : IDisposable" (untrimmed)
  children: Block[]; // parsed inner blocks (recursive)
  indent: string; // indentation of this declaration
}

interface LooseBlock {
  blockKind: "loose";
  lines: string[];
}

type Block = TypeBlock | LooseBlock;

interface BraceState {
  inBlockComment: boolean;
}

const countBraces = (line: string, state: BraceState): { open: number; close: number } => {
  let open = 0;
  let close = 0;
  let inString = false;
  let inChar = false;
  let inVerbatim = false;
  let i = 0;

  while (i < line.length) {
    const ch = line[i]!;
    const next = line[i + 1];

    if (state.inBlockComment) {
      if (ch === "*" && next === "/") {
        state.inBlockComment = false;
        i += 2;
        continue;
      }
      i++;
      continue;
    }

    if (ch === "/" && next === "*" && !inString && !inChar && !inVerbatim) {
      state.inBlockComment = true;
      i += 2;
      continue;
    }

    if (ch === "/" && next === "/" && !inString && !inChar && !inVerbatim) {
      break;
    }

    if (inVerbatim) {
      if (ch === '"') {
        if (next === '"') {
          i += 2;
          continue;
        }
        inVerbatim = false;
        i++;
        continue;
      }
      i++;
      continue;
    }

    if (inString) {
      if (ch === "\\") {
        i += 2;
        continue;
      }
      if (ch === '"') {
        inString = false;
      }
      i++;
      continue;
    }

    if (inChar) {
      if (ch === "\\") {
        i += 2;
        continue;
      }
      if (ch === "'") {
        inChar = false;
      }
      i++;
      continue;
    }

    if (ch === "@" && next === '"') {
      inVerbatim = true;
      i += 2;
      continue;
    }

    if (ch === '"') {
      inString = true;
      i++;
      continue;
    }

    if (ch === "'") {
      inChar = true;
      i++;
      continue;
    }

    if (ch === "{") open++;
    if (ch === "}") close++;
    i++;
  }

  return { open, close };
};

const TYPE_DECL_RE =
  /^(\s*)((?:(?:public|private|protected|internal|abstract|sealed|static|new)\s+)*)(?:(partial)\s+)?(class|struct|enum|interface|record)\s+(\w+(?:<[^>]+>)?)\s*(?::\s*([^{]+?))?\s*(\{.*\})?\s*$/;

const parseDeclarationLine = (line: string) => {
  const m = TYPE_DECL_RE.exec(line);
  if (!m) return null;

  const indent = m[1]!;
  const rawModifiers = m[2]!.trim();
  const isPartial = m[3] === "partial";
  const kind = m[4] as TypeBlock["kind"];
  const name = m[5]!;
  const baseRaw = m[6]?.trim();
  const inlineBody = m[7]?.trim(); // e.g. "{ }" or "{ ... }"

  const baseList = baseRaw
    ? baseRaw
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean)
    : [];

  return { indent, modifiers: rawModifiers, isPartial, kind, name, baseList, inlineBody };
};

/**
 * Parse lines into blocks at a given indentation level.
 * `expectedIndent` is the indentation string for type declarations at this level
 * (e.g. "    " for depth 1, "        " for depth 2).
 */
const parseBlocks = (lines: string[], expectedIndent: string): Block[] => {
  const blocks: Block[] = [];
  let looseLines: string[] = [];
  let i = 0;

  const flushLoose = () => {
    if (looseLines.length > 0) {
      blocks.push({ blockKind: "loose", lines: [...looseLines] });
      looseLines = [];
    }
  };

  const attrPattern = new RegExp(`^${escapeRegex(expectedIndent)}\\[`);

  while (i < lines.length) {
    // Try to match attribute lines followed by a type declaration
    const attrLines: string[] = [];
    let j = i;
    while (j < lines.length && attrPattern.test(lines[j]!)) {
      attrLines.push(lines[j]!);
      j++;
    }

    // Check if line at j is a type declaration at this indent level
    if (j < lines.length) {
      const parsed = parseDeclarationLine(lines[j]!);
      if (parsed && parsed.indent === expectedIndent) {
        flushLoose();

        const block: TypeBlock = {
          blockKind: "type",
          kind: parsed.kind,
          isPartial: parsed.isPartial,
          name: parsed.name,
          modifiers: parsed.modifiers,
          baseList: parsed.baseList,
          attributes: [...attrLines],
          constraint: null,
          children: [],
          indent: expectedIndent,
        };

        j++; // move past declaration line

        // Handle inline body like "public static partial class Components { }"
        if (parsed.inlineBody) {
          // Empty or inline body — no children to parse
          // Recursively parse body for nested type blocks
          const nestedIndent = expectedIndent + "    ";
          block.children = parseBlocks([], nestedIndent);

          blocks.push(block);
          i = j;
          continue;
        }

        // Check for where constraint
        if (j < lines.length && /^\s+where\s+/.test(lines[j]!)) {
          block.constraint = lines[j]!;
          j++;
        }

        // Find opening brace
        while (j < lines.length && lines[j]!.trim() !== "{") {
          j++;
        }
        j++; // skip opening {

        // Collect body lines until matching closing brace
        let innerDepth = 0;
        const bodyLines: string[] = [];
        const bodyState: BraceState = { inBlockComment: false };
        while (j < lines.length) {
          const bodyLine = lines[j]!;
          const braces = countBraces(bodyLine, bodyState);
          innerDepth += braces.open - braces.close;

          if (innerDepth < 0) {
            // closing brace of this type block
            break;
          }
          bodyLines.push(bodyLine);
          j++;
        }
        j++; // skip closing }

        // Recursively parse body for nested type blocks
        const nestedIndent = expectedIndent + "    ";
        block.children = parseBlocks(bodyLines, nestedIndent);

        blocks.push(block);
        i = j;
        continue;
      }
    }

    // Not a type declaration — if we collected attribute-like lines but they
    // aren't followed by a type decl, they're loose code
    if (attrLines.length > 0) {
      for (let k = i; k < Math.min(j, lines.length); k++) {
        looseLines.push(lines[k]!);
      }
      i = j;
      continue;
    }

    // Regular line
    looseLines.push(lines[i]!);
    i++;
  }

  flushLoose();
  return blocks;
};

const escapeRegex = (s: string): string => {
  return s.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
};

const deduplicateOrdered = (items: string[]): string[] => {
  const seen = new Set<string>();
  const result: string[] = [];
  for (const item of items) {
    if (!seen.has(item)) {
      seen.add(item);
      result.push(item);
    }
  }
  return result;
};

/**
 * Merge partial classes at the current level FIRST, then recurse into children.
 * This ensures that partials spread across different parent declarations
 * are combined before their children are recursively processed.
 */
const mergePartials = (blocks: Block[]): Block[] => {
  // Group partial classes by name
  const groups = new Map<string, TypeBlock[]>();

  for (const block of blocks) {
    if (block.blockKind !== "type" || !block.isPartial) continue;
    const key = block.name;
    if (!groups.has(key)) {
      groups.set(key, []);
    }
    groups.get(key)!.push(block);
  }

  // Build merged type blocks
  const merged = new Map<string, TypeBlock>();
  for (const [name, group] of groups) {
    const first = group[0]!;
    merged.set(name, {
      blockKind: "type",
      kind: first.kind,
      isPartial: false,
      name: first.name,
      modifiers: first.modifiers,
      baseList: deduplicateOrdered(group.flatMap((b) => b.baseList)),
      attributes: deduplicateOrdered(group.flatMap((b) => b.attributes)),
      constraint: group.find((b) => b.constraint)?.constraint ?? null,
      children: mergeChildren(group),
      indent: first.indent,
    });
  }

  // Build output: first occurrence → merged, subsequent → skip
  const seen = new Set<string>();
  const result: Block[] = [];

  for (const block of blocks) {
    if (block.blockKind === "loose") {
      result.push(block);
      continue;
    }

    if (!block.isPartial) {
      // Non-partial type: recursively merge its children
      const withMergedChildren =
        block.children.length > 0 ? { ...block, children: mergePartials(block.children) } : block;
      result.push(withMergedChildren);
      continue;
    }

    const key = block.name;
    if (!seen.has(key)) {
      seen.add(key);
      result.push(merged.get(key)!);
    }
    // else: skip subsequent occurrence
  }

  return result;
};

/**
 * Merge children from multiple partial declarations of the same class.
 * Concatenates all children in order, with separation, then re-merges
 * any partials that ended up together from different parent declarations.
 */
const mergeChildren = (group: TypeBlock[]): Block[] => {
  const combined: Block[] = [];
  for (let i = 0; i < group.length; i++) {
    if (i > 0 && group[i]!.children.length > 0 && combined.length > 0) {
      // Add blank line separator between bodies from different partial declarations
      combined.push({ blockKind: "loose", lines: [""] });
    }
    combined.push(...group[i]!.children);
  }
  // Re-merge because partial classes may be spread across different parent declarations
  return mergePartials(combined);
};

const emitBlock = (block: Block): string[] => {
  if (block.blockKind === "loose") {
    return [...block.lines];
  }

  const lines: string[] = [];

  // Attributes
  for (const attr of block.attributes) {
    lines.push(attr);
  }

  // Declaration line
  let decl = `${block.indent}${block.modifiers} ${block.kind} ${block.name}`;
  if (block.baseList.length > 0) {
    decl += ` : ${block.baseList.join(", ")}`;
  }
  lines.push(decl);

  // Constraint
  if (block.constraint) {
    lines.push(block.constraint);
  }

  // Opening brace
  lines.push(`${block.indent}{`);

  // Children
  for (const child of block.children) {
    lines.push(...emitBlock(child));
  }

  // Closing brace
  lines.push(`${block.indent}}`);

  return lines;
};

const emitBlocks = (blocks: Block[]): string[] => {
  const lines: string[] = [];
  for (const block of blocks) {
    lines.push(...emitBlock(block));
  }
  return lines;
};

const processFile = (lines: string[]): string[] => {
  // Find outer class declaration
  let outerDeclIndex = -1;
  for (let i = 0; i < lines.length; i++) {
    if (/^public\s+partial\s+class\s+MyCarbonoxide\b/.test(lines[i]!)) {
      outerDeclIndex = i;
      break;
    }
  }
  if (outerDeclIndex === -1) throw new Error("Could not find outer MyCarbonoxide class declaration");

  // Find opening brace of outer class
  let outerOpenBrace = -1;
  for (let i = outerDeclIndex; i < lines.length; i++) {
    if (lines[i]!.trim() === "{") {
      outerOpenBrace = i;
      break;
    }
  }
  if (outerOpenBrace === -1) throw new Error("Could not find opening brace of MyCarbonoxide class");

  // Find closing brace of outer class (last } in file at indent 0)
  let outerCloseBrace = -1;
  for (let i = lines.length - 1; i >= 0; i--) {
    if (lines[i]!.trim() === "}") {
      outerCloseBrace = i;
      break;
    }
  }
  if (outerCloseBrace === -1) throw new Error("Could not find closing brace of MyCarbonoxide class");

  // Split into sections
  const preamble = lines.slice(0, outerOpenBrace + 1);
  const innerLines = lines.slice(outerOpenBrace + 1, outerCloseBrace);
  const postamble = lines.slice(outerCloseBrace);

  // Remove partial from outer class
  preamble[outerDeclIndex] = preamble[outerDeclIndex]!.replace(/\bpartial\s+/, "");

  // Parse inner blocks recursively
  const blocks = parseBlocks(innerLines, "    ");

  // Merge partials at all levels
  const mergedBlocks = mergePartials(blocks);

  // Emit
  const output: string[] = [];
  output.push(...preamble);
  output.push(...emitBlocks(mergedBlocks));
  output.push(...postamble);

  return output;
};

const main = () => {
  const filePath = process.argv[2] ? resolve(process.argv[2]) : resolve(__dirname, "..", "MyCarbonoxide.cs");

  const content = readFileSync(filePath, "utf-8");
  const lines = content.split(/\r?\n/);

  const outputLines = processFile(lines);

  // Determine line ending from original file
  const lineEnding = content.includes("\r\n") ? "\r\n" : "\n";
  writeFileSync(filePath, outputLines.join(lineEnding), "utf-8");

  // Report
  const partialsBefore = lines.filter((l) => /\bpartial\s+class\b/.test(l)).length;
  const partialsAfter = outputLines.filter((l) => /\bpartial\s+class\b/.test(l)).length;
  console.log(
    `post-merge: ${partialsBefore} partial class declarations → ${partialsBefore - partialsAfter} merged (${partialsAfter} remaining)`,
  );
};

main();
