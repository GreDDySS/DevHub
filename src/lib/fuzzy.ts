export interface FuzzyResult {
  score: number;
  indices: number[];
}

const WORD_BOUNDARY = /[\s\-_./\\[(]/;

export function fuzzyMatch(query: string, text: string): FuzzyResult | null {
  const q = query.toLowerCase();
  const t = text.toLowerCase();

  if (!q) return { score: 0, indices: [] };
  if (q.length > t.length) return null;

  let score = 0;
  const indices: number[] = [];
  let searchFrom = 0;
  let prevIndex = -2;

  for (const ch of q) {
    const found = t.indexOf(ch, searchFrom);
    if (found === -1) return null;

    if (found === prevIndex + 1) score += 8;
    if (found === 0 || WORD_BOUNDARY.test(text[found - 1])) score += 6;

    score += 1;
    indices.push(found);
    prevIndex = found;
    searchFrom = found + 1;
  }

  score -= (text.length - query.length) * 0.05;
  return { score, indices };
}
