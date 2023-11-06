using System;
using System.Runtime.InteropServices.JavaScript;

namespace UP.Models;

public class FinancialOperations
{
    public int Id { get; set; }
    public int Client { get; set; }
    public int Sum { get; set; }
    public DateTimeOffset OperationDate { get; set; }
    public bool PaymentState { get; set; }
}