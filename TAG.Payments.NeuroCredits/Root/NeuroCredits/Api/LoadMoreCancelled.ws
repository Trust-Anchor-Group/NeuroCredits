AuthenticateSession(Request,"User");
Authorize(User,"Admin.Payments.NeuroCredits");

if !exists(Posted) then BadRequest("No data posted.");

Invoices:=select 
	top Posted.maxCount * 
from 
	TAG.Payments.NeuroCredits.Data.Invoice 
where 
	IsPaid=true 
order by 
	InvoiceNumber desc
offset
	Posted.offset;

[foreach Invoice in Invoices do
{
	"invoiceNumber": Invoice.InvoiceNumber,
	"name": (Invoice.IsOrganizational ? Invoice.OrganizationName : Invoice.PersonalName),
	"nr": (Invoice.IsOrganizational ? Invoice.OrganizationNumber : Invoice.PersonalNumber),
	"amountPaid": Invoice.AmountPaid,
	"currency": Invoice.Currency,
	"created": Invoice.Created.ToString(),
	"paid": Invoice.Paid.ToString(),
	"nrReminders": Invoice.NrReminders,
	"installment": Invoice.Installment,
	"nrInstallments": Invoice.NrInstallments,
	"contractId": Invoice.CancellationContractId
}]
