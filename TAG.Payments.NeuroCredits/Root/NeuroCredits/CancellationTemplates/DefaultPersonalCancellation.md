﻿Invoice{{if Invoice.NrInstallments>1 then " "+Str(Invoice.Installment) else ""}} cancelled
=============================================================================================

The following information contains information about the invoice that has been cancelled. The e-mail also contains a calendar entry 
named `Cancellation.ics`. It contains a cancellation of the reminder event in your calendar. By "adding" it to your calendar, you can
remove the invoice reminder provided with the original invoice.

| General Information                                      ||
|:----------------|----------------------------------------:|
| Invoice Number: | {{Invoice.InvoiceNumber}}               |
| Invoice Date:   | {{Invoice.Created.ToShortDateString()}} |
| Due Date:       | {{Invoice.DueDate.ToShortDateString()}} |
| Purpose:        | Purchase of Neuro-Credits™              |
{{

AddStringRow(Key,Value):=
(
	if !empty(Value) then ]]| ((Key)): | ((Value)) |
[[;
);

AddValueRow(Key,Value,Currency,Bold):=
(
	if Value!=0 then 
	(
		if Bold then
			]]| **((Key))**: | **((Value)) ((Currency))** |
[[
		else
			]]| ((Key)): | ((Value)) ((Currency)) |
[[
	)
);

if !empty(Invoice.Message) then AddStringRow("Message",Invoice.Message.Replace("\r\n","\n").Replace("\r","\n").Replace("&","&amp;").Replace("<","&lt;").Replace(">","&gt;").Replace("\n","<br/>"));

if Invoice.NrInstallments>1 then
	AddStringRow("Installment", Str(Invoice.Installment)+"/"+Str(Invoice.NrInstallments));

AddValueRow("Neuro-Credits™ purchased",Invoice.PurchaseAmount,Invoice.Currency,false);
AddValueRow("Invoice fee",Invoice.InvoiceFee,Invoice.Currency,false);
AddValueRow("Period interest",Invoice.PeriodInterest,"%",false);
AddValueRow("Amount",Invoice.Amount,Invoice.Currency,Invoice.LateFees=0 and Invoice.AmountPaid=0);
AddValueRow("Reminders",Invoice.NrReminders,"",false);

if Invoice.LateFees!=0 then
(
	AddValueRow("Late fees",Invoice.LateFees,Invoice.Currency,false);
	AddValueRow("Total",Invoice.TotalAmount,Invoice.Currency,true);
);
	
if Invoice.AmountPaid!=0 then
(
	AddValueRow("Already paid",Invoice.AmountPaid,Invoice.Currency,false);
	AddValueRow("Left",Invoice.AmountLeft,Invoice.Currency,true);
);
}}

| Recipient (Buyer)                                                                         ||
|:-----------------|:------------------------------------------------------------------------|
| Name:            | {{Invoice.PersonalName}}                                                |
| Personal Number: | {{Invoice.PersonalNumber}}                                              |
| Address:         | {{Invoice.Address}}{{empty(Invoice.Address2)?"":", "+Invoice.Address2}} |
{{
AddStringRow("Area",Invoice.Area);
AddStringRow("Postal Code",Invoice.PostalCode);
AddStringRow("City",Invoice.City);
AddStringRow("Region",Invoice.Region);
AddStringRow("Country",Invoice.Country);
}}| JID:             | [{{Invoice.Jid}}](xmpp:{{Invoice.Jid}})                               |
| Phone Number:    | [{{Invoice.PhoneNumber}}](tel:{{Invoice.PhoneNumber}})                  |
| e-Mail Address:  | [{{Invoice.EMail}}](mailto:{{Invoice.EMail}})                           |

| Sender (Seller), and payment information                                      ||
|:-----------------|:------------------------------------------------------------|
| Name:            | {{Billing.Sender}}                                          |
| Tax (VAT) Nr:    | {{Billing.TaxNr}}                                           |
| Contact e-mail:  | [{{Billing.ContactEMail}}](mailto:{{Billing.ContactEMail}}) |
{{
if !empty(Billing.ContactPhone) then AddStringRow("Contact phone","["+Billing.ContactPhone+"](tel:"+Billing.ContactPhone+")");
if !empty(Billing.Reference) then AddStringRow("Sender reference",Billing.Reference);
AddStringRow("Address",Billing.SenderAddress);
AddStringRow("Country",Billing.SenderCountry);
if !empty(Billing.WebPage) then AddStringRow("WebPage","<"+Billing.WebPage+">");
AddStringRow("Bank Account",Billing.BankAccount);
AddStringRow("IBAN",Billing.Iban);
AddStringRow("Account holder's name",Billing.BankAccountName);
AddStringRow("BIC or SWIFT",Billing.BankIdentifier);
AddStringRow("Name of Bank",Billing.BankName);
AddStringRow("Bank Branch address",Billing.BranchAddress);
}}
