using System.Globalization;
using Application.Models;
using Domain.Enums;

namespace Application.Services;

public class CnabParser : ICnabParser
{
    private const int PosType = 0;      private const int LenType = 1;
    private const int PosDate = 1;      private const int LenDate = 8;
    private const int PosAmount = 9;    private const int LenAmount = 10;
    private const int PosCpf = 19;      private const int LenCpf = 11;
    private const int PosCard = 30;     private const int LenCard = 12;
    private const int PosTime = 42;     private const int LenTime = 6;
    private const int PosOwner = 48;    private const int LenOwner = 14;
    private const int PosStore = 62;    private const int LenStore = 19;

    public CnabLineResult ParseLine(ReadOnlySpan<char> line)
    {
        var typeRaw = int.Parse(line.Slice(PosType, LenType));
        var type = (TransactionType)typeRaw;
        
        var dateSlice = line.Slice(PosDate, LenDate);
        var date = DateTime.ParseExact(dateSlice, "yyyyMMdd", CultureInfo.InvariantCulture);
        
        var timeSlice = line.Slice(PosTime, LenTime);
        var time = TimeSpan.ParseExact(timeSlice, "hhmmss", CultureInfo.InvariantCulture);
        
        var dateTime = new DateTimeOffset(date.Add(time), TimeSpan.FromHours(-3));
        
        var amountRaw = long.Parse(line.Slice(PosAmount, LenAmount));
        var amount = amountRaw / 100m;
        
        var cpf = line.Slice(PosCpf, LenCpf).ToString();
        
        var cardNumber = line.Slice(PosCard, LenCard).Trim().ToString();
        
        var ownerName = line.Slice(PosOwner, LenOwner).Trim().ToString();
        
        var storeName = line.Slice(PosStore, Math.Min(LenStore, line.Length - PosStore)).Trim().ToString();

        return new CnabLineResult(
            Type: type,
            Date: dateTime,
            Amount: amount,
            Cpf: cpf,
            CardNumber: cardNumber,
            StoreName: storeName,
            StoreOwnerName: ownerName
        );
    }
}
