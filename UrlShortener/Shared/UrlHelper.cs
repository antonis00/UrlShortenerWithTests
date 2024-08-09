namespace UrlShortener.Shared;

public class UrlHelper
{
    private const string _baseUrl = "abc.com/";

    public static string Strip(string url)
    {
        int index = url.IndexOf(_baseUrl);

        if (index >= 0)
        {
            url = url.Substring(index + _baseUrl.Length);
        }

        url = url.TrimEnd('/');

        return url;
    }

    public static string Wrap(string url)
    {
        return $"https://abc.com/{url}";
    }


}
