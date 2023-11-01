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

![Invoice cancellation link]({{
PayUrl:="iotsc:"+TAG.Payments.NeuroCredits.NeuroCreditsService.SellEDalerTemplateId;
PayUrl+="?Amount="+Str(Invoice.Amount);
PayUrl+="&Currency="+Str(Invoice.Currency);
Waher.IoTGateway.Gateway.GetUrl("/QR/" + UrlEncode(PayUrl))}})
