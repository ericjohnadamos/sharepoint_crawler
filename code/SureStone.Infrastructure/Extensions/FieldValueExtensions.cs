namespace Insurance.Infrastructure.Extensions;

public static class FieldValueExtensions
{
    public static string IsMicrosoftSupportedExtensionAsString(this object? fieldValue)
    {
        return IsMicrosoftSupportedType(fieldValue?.ToString()).ToString();
    }

    private static bool IsMicrosoftSupportedType(string? type)
    {
        if (type == null)
            return false;

        var microsoftExtensions = new List<string>
        {
            // Microsoft Word
            @"doc",
            @"docx",
            @"dot",
            @"dotx",
            @"docm",
            @"dotm",
            @"word",
            @"w6w",

            // Microsoft Excel
            @"xls",
            @"xlsx",
            @"xlt",
            @"xltx",
            @"xla",
            @"xlw",
            @"xlsm",
            @"xlsb",
            @"xltm",
            @"xlam",
            @"csv",

            // Microsoft PowerPoint
            @"ppt",
            @"pptx",
            @"pot",
            @"potx",
            @"pps",
            @"ppsx",
            @"ppa",
            @"ppam",
            @"pptm",
            @"ppsm",
            @"potm",

            // Microsoft Access
            @"mdb",
            @"accda",
            @"accdb",
            @"accde",
            @"accdr",
            @"accdt",
            @"ade",
            @"adp",
            @"adn",
            @"mde",
            @"mdf",
            @"mdn",
            @"mdt",
            @"mdw",

            // Other
            @"calcx",
            @"hlp",
            @"mpp",
            @"thmx",
            @"wri",
            @"ico",
            @"vsd",
            @"application",
            @"manifest",
            @"deploy",
            @"msp",
            @"msu",
            @"vsto",
            @"xaml",
            @"xbap",
        };

        return microsoftExtensions.Any(e => e == type);
    }
}
