const pkg = require('yahoo-finance2');
const YahooFinance = pkg.YahooFinance || pkg.default || pkg;
const yahooFinance = new YahooFinance({ suppressNotices: ['yahooSurvey', 'ripHistorical'] });
const fs = require('fs');

async function exportDailyMarketCap(ticker, years = 10) {
  try {
    // 1. Récupération des actions en circulation
    const quote = await yahooFinance.quote(ticker);
    const sharesOutstanding = quote.sharesOutstanding;

    if (!sharesOutstanding) {
      throw new Error("Impossible de récupérer le nombre d'actions en circulation.");
    }

    // 2. Plage de dates
    const endDate = new Date();
    const startDate = new Date();
    startDate.setFullYear(endDate.getFullYear() - years);

    // 3. Récupération des cours au format quotidien ('1d')
    const result = await yahooFinance.chart(ticker, {
      period1: startDate,
      period2: endDate,
      interval: '1d'
    });

    const quotes = result.quotes || [];

    // 4. Construction du CSV (filtrage des week-ends et jours fériés sans cotation)
    const header = 'Date,Close,Shares_Outstanding,Market_Cap\n';
    const rows = quotes
      .filter(row => row.close != null)
      .map(row => {
        const date = new Date(row.date).toISOString().split('T')[0];
        const close = row.close.toFixed(2);
        const marketCap = (row.close * sharesOutstanding).toFixed(0);
        return `${date},${close},${sharesOutstanding},${marketCap}`;
      })
      .join('\n');

    // 5. Écriture du fichier CSV
    const fileName = `${ticker}_daily_market_cap_${years}y.csv`;
    fs.writeFileSync(fileName, header + rows, 'utf-8');
    console.log(`Succès : ${quotes.length} jours enregistrés dans ${fileName}`);

  } catch (error) {
    console.error('Erreur :', error.message);
  }
}

exportDailyMarketCap('AAPL', 20);
exportDailyMarketCap('IBM', 20);
exportDailyMarketCap('MSFT', 20);
exportDailyMarketCap('GOOGL', 20);
exportDailyMarketCap('AMZN', 20);
exportDailyMarketCap('META', 20);
exportDailyMarketCap('TSM', 20);
exportDailyMarketCap('AVGO', 20);
exportDailyMarketCap('TSLA', 20);

