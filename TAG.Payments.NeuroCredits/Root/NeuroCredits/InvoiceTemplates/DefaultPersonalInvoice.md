Invoice{{if Invoice.NrInstallments>1 then " "+Str(Invoice.Installment) else ""}}
====================================================================================

The following information contains an invoice for the Neuro-Credits™ that was purchased. You will find a QR-encoded link below
with options to pay the corresponding invoice.

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

if Invoice.NrInstallments>1 then
	AddStringRow("Installment", Str(Invoice.Installment)+"/"+Str(Invoice.NrInstallments));

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
if !empty(Billing.ContactPhone) then AddStringRow("Contact phone","<tel:"+Billing.ContactPhone+">");
AddStringRow("Bank Account",Billing.BankAccount);
AddStringRow("IBAN",Billing.Iban);
AddStringRow("Account holder's name",Billing.BankAccountName);
AddStringRow("BIC or SWIFT",Billing.BankIdentifier);
AddStringRow("Name of Bank",Billing.BankName);
AddStringRow("Branch address",Billing.BranchAddress);
}}

{{
]]```[[;
PayUrl:="iotsc:"+TAG.Payments.NeuroCredits.NeuroCreditsService.SellEDalerTemplateId;
PayUrl+="?Amount="+Str(Invoice.Amount);
PayUrl+="&Currency="+Str(Invoice.Currency);
Image:=QrEncode(PayUrl,"H",400);
Encoded:=Encode(Image);
]]((Encoded[1])):Invoice cancellation link
((Base64Encode(Encoded[0]);))
```
[[
}}
