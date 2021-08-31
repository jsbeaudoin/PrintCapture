namespace PrintsCapture.Prints.Enum
{
    public enum TemplateError
    {        
        WsqNotCreated = -100,
        ErrorOnExtract = -1,     
        Unkown = 0,
        TooFewMinutiae = 90,
        QualityCheckFailed = 100,
        MatchingFailed = 200,
        WrongTemplateCount = 1024,
        TemplatePositionUnknown = 2048,
        WrongHand = 4096
    }
}