Invoice{{if Invoice.NrInstallments>1 then " "+Str(Invoice.Installment) else ""}}
====================================================================================

The following information contains an invoice for the Neuro-Credits™ that was purchased. You will find a QR-encoded link below
with options to pay the corresponding invoice. The e-mail also contains a calendar entry named `Reminder.ics`. You can add this
to your calendar, to receive a payment notification before the invoice is due.

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

| Recipient (Buyer)                                                                                                                 ||
|:---------------------|:------------------------------------------------------------------------------------------------------------|
| Name:                | {{Invoice.OrganizationName}}                                                                                |
| Organization Number: | {{Invoice.OrganizationNumber}}                                                                              |
| Address:             | {{Invoice.OrganizationAddress}}{{empty(Invoice.OrganizationAddress2)?"":", "+Invoice.OrganizationAddress2}} |
{{
AddStringRow("Area",Invoice.OrganizationArea);
AddStringRow("Postal Code",Invoice.OrganizationPostalCode);
AddStringRow("City",Invoice.OrganizationCity);
AddStringRow("Region",Invoice.OrganizationRegion);
AddStringRow("Country",Invoice.OrganizationCountry);
}}

| Personal reference at Buyer                                                               ||
|:-----------------|:------------------------------------------------------------------------|
| Name:            | {{Invoice.PersonalName}}                                                |
| Personal Number: | {{Invoice.PersonalNumber}}                                              |
| Department:      | {{Invoice.Department}}                                                  |
| Role:            | {{Invoice.Role}}                                                        |
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

[Payment link]({{Waher.IoTGateway.Gateway.GetUrl("/NeuroCredits/PayInvoice.md?Nr="+Str(Invoice.InvoiceNumber)+"&Key="+UrlEncode(Invoice.Key))}}), if paying via browser.

{{
]]```[[;
PayUrl:="iotsc:"+TAG.Payments.NeuroCredits.NeuroCreditsService.SellEDalerTemplateId;
PayUrl+="?Amount="+Str(Invoice.Amount);
PayUrl+="&Currency="+Str(Invoice.Currency);
Image:=QrEncode(PayUrl,"H",400);
Encoded:=Encode(Image);
]]((Encoded[1])):Payment link, if paying via App
((Base64Encode(Encoded[0]);))
```
[[
}}
