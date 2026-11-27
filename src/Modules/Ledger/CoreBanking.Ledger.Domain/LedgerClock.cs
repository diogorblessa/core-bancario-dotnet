namespace CoreBanking.Ledger.Domain;

public static class LedgerClock
{
    private static readonly TimeZoneInfo SaoPaulo = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static DateOnly Today(TimeProvider time)
    {
        var nowInSaoPaulo = TimeZoneInfo.ConvertTime(time.GetUtcNow(), SaoPaulo);

        return DateOnly.FromDateTime(nowInSaoPaulo.DateTime);
    }
}
