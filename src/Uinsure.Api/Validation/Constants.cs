namespace Uinsure.Api.Validation;

public static class Constants
{
    public const int SELL_MAX_DAYS_BEFORE_START = 60;
    public const int SELL_POLICY_DURATION = 1; // years
    public const int SELL_MIN_POLICY_HOLDERS = 1;
    public const int SELL_MAX_POLICY_HOLDERS = 3;
    public const int SELL_MIN_POLICY_HOLDER_AGE = 16; // years

    public const int RENEW_MAX_DAYS_BEFORE_END = 30;

    public const int CANCEL_COOL_OFF_DAYS = 14;
}
