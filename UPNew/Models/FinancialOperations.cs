using System;
using System.Runtime.InteropServices.JavaScript;

namespace UP.Models;

public class FinancialOperations
{
    public int Id { get; set; }
    public int Client { get; set; }
    public int Sum { get; set; }
    public DateTimeOffset OperationDate { get; set; }
    public bool PaymentState;

    public string PS
    {
        get
        {
            if (PaymentState)
            {
                return "Paid";
            }
            else
            {
                return "Not paid";
            }
        }
    }
    public string ClientName { get; set; }
    public string ClientSurname { get; set; }
}