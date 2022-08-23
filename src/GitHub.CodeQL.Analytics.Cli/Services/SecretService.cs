using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GitHub.CodeQL.Analytics.Cli.Services; 

public class SecretService {

    private readonly byte[] _key;

    public SecretService(ILogger<SecretService> log, IConfiguration config) {
        var keyAsHex = config.GetValue<string>("Security:Key");
        if (string.IsNullOrEmpty(keyAsHex)) {
            log.LogWarning("key has not been defined for cryptographic operations");
            return;
        }
        _key = Convert.FromHexString(keyAsHex);
    }

    public string Encrypt(string plainText) {
        var random = new Random();
        var nonce = new byte[12];
        
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherTextBytes = new Span<byte>(plainTextBytes);
        var tag = new byte[16];
        
        random.NextBytes(nonce);
        
        var cipher = new AesGcm(_key);
        cipher.Encrypt(
            nonce,
            plainTextBytes,
            cipherTextBytes,
            tag
        );
        
        var resultAsByteArray = nonce.Concat(cipherTextBytes.ToArray()).Concat(tag);
        return Convert.ToBase64String(resultAsByteArray.ToArray());
    }

    public string Decrypt(string encryptedData) {
        var data = Convert.FromBase64String(encryptedData);

        const int nonceLength = 12;
        var nonce = new ReadOnlySpan<byte>(data, 0, nonceLength);

        const int tagLength = 16;
        var tagStart = data.Length - tagLength;
        var tag = new ReadOnlySpan<byte>(data, tagStart, tagLength);

        const int cipherStart = nonceLength;
        var cipherTextLength = data.Length - tagLength - nonceLength;
        var cipherTextBytes = new ReadOnlySpan<byte>(data, cipherStart, cipherTextLength);

        var plainTextBytes = new byte[cipherTextLength];

        var cipher = new AesGcm(_key);
        cipher.Decrypt(
            nonce,
            cipherTextBytes,
            tag,
            plainTextBytes
        );

        return Encoding.UTF8.GetString(plainTextBytes);
    }
    
}