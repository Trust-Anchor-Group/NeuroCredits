AuthenticateSession(Request,"User");
Authorize(User,"Admin.Payments.NeuroCredits");

({
	"invoiceNr":Optional(Number(PInvoiceNr))
}:=Posted) ??? BadRequest("Invalid content in request.");

Invoice:=select top 1 * from TAG.Payments.NeuroCredits.Data.Invoice where InvoiceNumber=PInvoiceNr;
if !exists(Invoice) then NotFound("Invoice is not found.");

Invoice.Resend();
true