Title: Accounts
Description: Configures account-specific settings for the Neuro-Credits™ service.
Date: 2023-10-25
Author: Peter Waher
Master: /Master.md
Javascript: Settings.js
Javascript: /Events.js
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Privilege: Admin.Payments.NeuroCredits
Login: /Login.md

========================================================================

Account Settings
====================

Below you can edit **individual account** settings for the Neuro-Credits™ service. These settings will be used for the corresponding accounts,
instead of the default settings provided.

<form action="Accounts.md" method="post">
<fieldset id="IndividualAuthorizations">
<legend>Accounts</legend>

| Account | Personal Number | Country | Amount | Currency | Period | Interest | Max Installments | Debt |   |   |
|:--------|:----------------|:--------|-------:|:---------|-------:|---------:|-----------------:|-----:|:-:|:-:|
{{
Accounts:=select * from TAG.Payments.NeuroCredits.Data.AccountConfiguration order by Account;
foreach Account in Accounts do
(
	]]| `((Account.Account))` [[;
	]]| ((Account.PersonalNumber)) [[;
	]]| ((Account.Country)) [[;
	]]| ((Account.MaxCredit)) [[;
	]]| ((Account.Currency)) [[;
	]]| ((Account.Period)) [[;
	]]| ((Account.PeriodInterest)) % [[;
	]]| ((Account.MaxInstallments)) [[;
	]]| ((TAG.Payments.NeuroCredits.ServiceConfiguration.CurrentPersonalDebt(Account.Account,Account.PersonalNumber,Account.Country);)) [[;
	]]| <button type="button" class="posButtonSm" data-objectid="((Account.ObjectId))" onclick="EditAccount(this)">Edit</button> [[;
	]]| <button type="button" class="negButtonSm" data-objectid="((Account.ObjectId))" onclick="DeleteAccount(this)">Delete</button> [[;
	]]|
[[
)
}}

<input id="DefaultPersonalLimit" type="hidden" value="{{GetSetting('TAG.Payments.NeuroCredits.DefaultPersonalLimit',0)}}"/>
<input id="DefaultCurrency" type="hidden" value="{{GetSetting('DefaultCurrency','')}}"/>
<input id="DefaultPeriod" type="hidden" value="{{GetSetting('TAG.Payments.NeuroCredits.Period','P1M')}}"/>
<input id="DefaultPeriodInterest" type="hidden" value="{{GetSetting('TAG.Payments.NeuroCredits.PeriodInterest',2)}}"/>
<input id="DefaultMaxInstallments" type="hidden" value="{{GetSetting('TAG.Payments.NeuroCredits.MaxInstallments',6)}}"/>

<button type="button" class="posButton" onclick="AddAccount()">Add Account</button>
<button type="button" class="negButton" onclick="Close()">Close</button>

</fieldset>
</form>
