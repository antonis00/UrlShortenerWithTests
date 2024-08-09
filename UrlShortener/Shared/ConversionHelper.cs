using System.Text;
using UrlShortener.Models;

namespace UrlShortener.Shared;

public static class ConversionHelper
{

    public static Url EncodeUrl(string longUrl)
    {
        var guid = Guid.NewGuid();

        return new Url
        {
            OriginalUrl = longUrl,
            ShortUrl = Base62Encode(guid)
        };
    }

    private static string Base62Encode(Guid guid)
    {
        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        var bytes = guid.ToByteArray();
        ulong intData = BitConverter.ToUInt64(bytes, 0) ^ BitConverter.ToUInt64(bytes, 8);
        var result = new StringBuilder();

        while (intData > 0)
        {
            result.Append(chars[(int)(intData % 62)]);
            intData /= 62;
        }

        return new string(result.ToString().Reverse().ToArray());
    }
}
