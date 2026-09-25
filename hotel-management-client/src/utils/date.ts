const pad = (n: number) => String(n).padStart(2, "0");

export function toDateInput(d: Date): string {
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

export function addDays(dateStr: string, days: number): string {
  const [y, m, d] = dateStr.split("-").map(Number);
  return toDateInput(new Date(y, m - 1, d + days));
}

export const todayInput = () => toDateInput(new Date());

export const firstOfMonthInput = () => {
  const n = new Date();
  return toDateInput(new Date(n.getFullYear(), n.getMonth(), 1));
};