Title: Pending Invoices
Description: Shows pending invoices
Date: 2023-10-31
Author: Peter Waher
Master: /Master.md
Javascript: Settings.js
Javascript: /Events.js
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Privilege: Admin.Payments.NeuroCredits
Login: /Login.md

========================================================================

Pending invoices
====================

Below you can review pending invoices.

| Inv.Nr | Recipient | Pers./Org.Nr | Total | Left | Currency | Created | Due | \#Reminders | Installment | Contract |
|-------:|:----------|:------------:|------:|-----:|:---------|:-------:|:---:|------------:|:-----------:|:---------|
{{
Invoices:=select 
	top 20 * 
from 
	TAG.Payments.NeuroCredits.Data.Invoice 
where 
	IsPaid=false 
order by 
	DueDate;

foreach Invoice in Invoices do
(
	]]| ((Invoice.InvoiceNumber)) [[;
	]]| ((Invoice.IsOrganizational ? Invoice.OrganizationName : Invoice.PersonalName)) [[;
	]]| ((Invoice.IsOrganizational ? Invoice.OrganizationNumber : Invoice.PersonalNumber)) [[;
	]]| ((Invoice.TotalAmount)) [[;
	]]| ((Invoice.AmountLeft)) [[;
	]]| ((Invoice.Currency)) [[;
	]]| ((Invoice.Created)) [[;
	]]| ((Invoice.DueDate.ToShortDateString();)) [[;
	]]| ((Invoice.NrReminders)) [[;
	]]| ((Invoice.Installment))/((Invoice.NrInstallments)) [[;
	]]| ((empty(Invoice.NeuroCreditsContractId)?"":"<a href=\"/Contract.md?ID="+Invoice.NeuroCreditsContractId+"\" target=\"_blank\">Contract</a>")) [[;
	]]|
[[
);

if count(Invoices)==20 then ]]

<button id="LoadMoreButton" class='posButton' type="button" onclick='LoadMore(this,20,20,"Pending")'>Load More</button>
[[

}}

<button type="button" class="negButton" onclick="Close()">Close</button>
