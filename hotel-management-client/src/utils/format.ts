export const formatMoney = (value: number) =>
  value.toLocaleString("uk-UA", { minimumFractionDigits: 2, maximumFractionDigits: 2 });


export const formatDate = (iso: string) => {
  const [y, m, d] = iso.split("-");
  return `${d}.${m}.${y}`;
};