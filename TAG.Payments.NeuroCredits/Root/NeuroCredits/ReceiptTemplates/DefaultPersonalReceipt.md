Receipt
===========

The following information contains the receipt for the purchase of Neuro-Credits™. You will find a QR-encoded link below
with a reference to the purchase signed contract, for further information.

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

AddValueRow("Neuro-Credits™ purchased",Invoice.PurchaseAmount,Invoice.Currency,false);
AddValueRow("Installments",Invoice.NrInstallments,"",false);
AddValueRow("Price",Invoice.PurchasePrice,Invoice.Currency,true);
}}

| Information about Buyer                                                                   ||
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

{{
if !empty(Invoice.NeuroCreditsContractId) then 
(
	]]

![Purchase Contract, for Reference]([[;

	]]((Waher.IoTGateway.Gateway.GetUrl("/QR/nfeat:"+Invoice.NeuroCreditsContractId);))[[;

	]])

[[;
);
}}
