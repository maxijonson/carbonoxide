// Plugin.Merge leaves #if directives inside type bodies as-is, so PREMIUM regions get resolved here.
// Other directives (like #if CARBON) are left for the server compiler.

interface Frame {
  premium: boolean;
  keeping: boolean;
}

const IF_PREMIUM_RE = /^#if\s+(!?)PREMIUM\s*(\/\/.*)?$/;
const OTHER_DIRECTIVE_RE = /^#(if|elif|else|endif|define|undef)\b/;

export const resolvePremiumDirectives = (lines: string[], premiumDefined: boolean): string[] => {
  const output: string[] = [];
  const stack: Frame[] = [];
  const dropping = () => stack.some((frame) => frame.premium && !frame.keeping);

  for (const line of lines) {
    const directive = line.trim();

    const ifPremium = IF_PREMIUM_RE.exec(directive);
    if (ifPremium) {
      const negated = ifPremium[1] === "!";
      stack.push({ premium: true, keeping: negated !== premiumDefined });
      continue;
    }

    if (OTHER_DIRECTIVE_RE.test(directive)) {
      const top = stack[stack.length - 1];
      if (top?.premium && /^#(elif|else|endif)\b/.test(directive)) {
        if (directive.startsWith("#elif")) {
          throw new Error(`#elif is not supported on a PREMIUM region: "${directive}"`);
        }
        if (directive.startsWith("#else")) {
          top.keeping = !top.keeping;
        } else {
          stack.pop();
        }
        continue;
      }
      if (/\bPREMIUM\b/.test(directive)) {
        throw new Error(
          `Unsupported PREMIUM directive: "${directive}" (only plain "#if PREMIUM" and "#if !PREMIUM" are supported)`,
        );
      }
      if (/^#if\b/.test(directive)) {
        stack.push({ premium: false, keeping: true });
      } else if (directive.startsWith("#endif")) {
        if (!stack.pop()) {
          throw new Error("#endif without matching #if");
        }
      }
    }

    if (!dropping()) {
      output.push(line);
    }
  }

  if (stack.length > 0) {
    throw new Error("Unclosed #if region at end of file");
  }
  return output;
};
