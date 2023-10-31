AuthenticateSession(Request,"User");
Authorize(User,"Admin.Payments.NeuroCredits");

if !exists(Posted) then BadRequest("No data posted.");

Invoices:=select 
	top Posted.maxCount * 
from 
	TAG.Payments.NeuroCredits.Data.Invoice 
where 
	IsPaid=false
order by 
	DueDate
offset
	Posted.offset;

[foreach Invoice in Invoices do
{
	"invoiceNumber": Invoice.InvoiceNumber,
	"name": (Invoice.IsOrganizational ? Invoice.OrganizationName : Invoice.PersonalName),
	"nr": (Invoice.IsOrganizational ? Invoice.OrganizationNumber : Invoice.PersonalNumber),
	"total": Invoice.TotalAmount,
	"left": Invoice.AmountLeft,
	"currency": Invoice.Currency,
	"created": Invoice.Created.ToString(),
	"due": Invoice.DueDate.ToShortDateString(),
	"nrReminders": Invoice.NrReminders,
	"installment": Invoice.Installment,
	"nrInstallments": Invoice.NrInstallments,
	"contractId": Invoice.NeuroCreditsContractId
}]
