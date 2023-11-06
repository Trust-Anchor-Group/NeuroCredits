Receipt
===========

The following information contains the receipt for the purchase of Neuro-Credits™. You will find a QR-encoded link below
with a reference to the signed purchase contract, for further information.

| General Information                                      ||
|:-----------------|----------------------------------------:|
| Purchase Date:   | {{Invoice.Created.ToShortDateString()}} |
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

AddValueRow("Neuro-Credits™ purchased",Invoice.PurchaseAmount,Invoice.Currency,false);
AddValueRow("Installments",Invoice.NrInstallments,"",false);
AddValueRow("Price",Invoice.PurchasePrice,Invoice.Currency,true);
AddValueRow("Invoice fee",Invoice.InvoiceFee,Invoice.Currency,false);
AddValueRow("Period interest",Invoice.PeriodInterest,"%",false);
}}

| Information about Buyer                                                                                                           ||
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

{{
if !empty(Invoice.NeuroCreditsContractId) then 
(
	]]```[[;
	Image:=QrEncode("nfeat:"+Invoice.NeuroCreditsContractId,"H",400);
	Encoded:=Encode(Image);
	]]((Encoded[1])):Purchase Contract, for Reference
((Base64Encode(Encoded[0]);))
```
[[
);
}}
