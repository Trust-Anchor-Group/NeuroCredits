function AddAccount()
{
	var Table = FindTable("IndividualAuthorizations");
	var Body = FindChild(Table, "TBODY");
	var Tr = document.createElement("TR");
	Body.appendChild(Tr);

	var Td = document.createElement("TD");
	Tr.appendChild(Td);

	var Input = document.createElement("INPUT");
	Input.setAttribute("type", "text");
	Input.setAttribute("name", "Account");
	Input.setAttribute("required", "required");
	Td.appendChild(Input);
	Input.focus();

	Td = document.createElement("TD");
	Tr.appendChild(Td);

	Input = document.createElement("INPUT");
	Input.setAttribute("type", "text");
	Input.setAttribute("name", "PNr");
	Input.setAttribute("required", "required");
	Td.appendChild(Input);

	Td = document.createElement("TD");
	Tr.appendChild(Td);

	Input = document.createElement("INPUT");
	Input.setAttribute("type", "text");
	Input.setAttribute("name", "Country");
	Input.setAttribute("pattern", "^[A-Z]{2}$");
	Input.setAttribute("required", "required");
	Td.appendChild(Input);

	Td = document.createElement("TD");
	Td.setAttribute("style", "text-align:right");
	Tr.appendChild(Td);

	Input = document.createElement("INPUT");
	Input.setAttribute("type", "number");
	Input.setAttribute("name", "Amount");
	Input.setAttribute("min", "0");
	Input.setAttribute("value", document.getElementById("DefaultPersonalLimit").value);
	Input.setAttribute("required", "required");
	Td.appendChild(Input);

	Td = document.createElement("TD");
	Tr.appendChild(Td);

	Input = document.createElement("INPUT");
	Input.setAttribute("type", "text");
	Input.setAttribute("name", "Currency");
	Input.setAttribute("pattern", "^[A-Z]{3}$");
	Input.setAttribute("value", document.getElementById("DefaultCurrency").value);
	Input.setAttribute("required", "required");
	Td.appendChild(Input);

	Td = document.createElement("TD");
	Td.setAttribute("style", "text-align:right");
	Tr.appendChild(Td);

	Input = document.createElement("INPUT");
	Input.setAttribute("type", "text");
	Input.setAttribute("name", "Period");
	Input.setAttribute("pattern", "^(-?)P(?=\\d|T\\d)(?:(\\d+)Y)?(?:(\\d+)M)?(?:(\\d+)([DW]))?(?:T(?:(\\d+)H)?(?:(\\d+)M)?(?:(\\d+(?:\\.\\d+)?)S)?)?$");
	Input.setAttribute("value", document.getElementById("DefaultPeriod").value);
	Input.setAttribute("required", "required");
	Td.appendChild(Input);

	Td = document.createElement("TD");
	Td.setAttribute("style", "text-align:right");
	Tr.appendChild(Td);

	Input = document.createElement("INPUT");
	Input.setAttribute("type", "number");
	Input.setAttribute("name", "Interest");
	Input.setAttribute("min", "0");
	Input.setAttribute("value", document.getElementById("DefaultPeriodInterest").value);
	Input.setAttribute("required", "required");
	Td.appendChild(Input);

	Td = document.createElement("TD");
	Td.setAttribute("style", "text-align:right");
	Tr.appendChild(Td);

	Input = document.createElement("INPUT");
	Input.setAttribute("type", "number");
	Input.setAttribute("name", "MaxInstallments");
	Input.setAttribute("min", "1");
	Input.setAttribute("max", "12");
	Input.setAttribute("value", document.getElementById("DefaultMaxInstallments").value);
	Input.setAttribute("required", "required");
	Td.appendChild(Input);

	Td = document.createElement("TD");
	Td.setAttribute("style", "text-align:right");
	Tr.appendChild(Td);

	Td = document.createElement("TD");
	Tr.appendChild(Td);

	var Button = document.createElement("BUTTON");
	Button.setAttribute("type", "button");
	Button.setAttribute("class", "posButtonSm");
	Button.setAttribute("onclick", "OkAccount(this)");
	Button.innerText = "Add";
	Td.appendChild(Button);

	Td = document.createElement("TD");
	Tr.appendChild(Td);

	Button = document.createElement("BUTTON");
	Button.setAttribute("type", "button");
	Button.setAttribute("class", "negButtonSm");
	Button.setAttribute("onclick", "CancelAccount(this)");
	Button.innerText = "Cancel";
	Td.appendChild(Button);
}

function OkAccount(Control)
{
	var Td = Control.parentNode;
	var Tr = Td.parentNode;
	var Loop;
	var i;

	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
			{
				try
				{
					var Data = JSON.parse(xhttp.responseText);

					Loop = Tr.firstChild;
					i = 0;

					while (Loop !== null)
					{
						if (Loop.tagName === "TD")
						{
							switch (i++)
							{
								case 0:
									Loop.innerHTML = "";
									var Code = document.createElement("CODE");
									Code.innerText = Data.Account;
									Loop.appendChild(Code);
									break;

								case 1:
									Loop.innerText = Data.PersonalNumber;
									break;

								case 2:
									Loop.innerText = Data.Country;
									break;

								case 3:
									Loop.innerText = Data.MaxCredit;
									break;

								case 4:
									Loop.innerText = Data.Currency;
									break;

								case 5:
									Loop.innerText = Data.Period;
									break;

								case 6:
									Loop.innerText = Data.PeriodInterest + " %";
									break;

								case 7:
									Loop.innerText = Data.MaxInstallments;
									break;

								case 8:
									GetAccountDebt(Loop, Data.Account);
									break;

								case 9:
									Loop.innerHTML = "";
									var Button = document.createElement("BUTTON");
									Button.setAttribute("type", "button");
									Button.setAttribute("class", "posButtonSm");
									Button.setAttribute("data-objectid", Data.ObjectId);
									Button.setAttribute("onclick", "EditAccount(this)");
									Button.innerText = "Edit";
									Loop.appendChild(Button);
									break;

								case 10:
									Loop.innerHTML = "";
									Button = document.createElement("BUTTON");
									Button.setAttribute("type", "button");
									Button.setAttribute("class", "negButtonSm");
									Button.setAttribute("data-objectid", Data.ObjectId);
									Button.setAttribute("onclick", "DeleteAccount(this)");
									Button.innerText = "Delete";
									Loop.appendChild(Button);
									break;
							}
						}

						Loop = Loop.nextSibling;
					}
				}
				catch (e)
				{
					console.log(e);
					console.log(xhttp.responseText);
				}
			}
			else
				ShowError(xhttp);
		};
	}

	try
	{
		Loop = Tr.firstChild;
		i = 0;
		var Request = {
			"ObjectId": Control.getAttribute("data-objectid")
		};

		while (Loop !== null)
		{
			if (Loop.tagName === "TD")
			{
				var Input = FindChild(Loop, "INPUT");

				if (Input)
				{
					switch (i++)
					{
						case 0:
							if (Input.value.length > 0)
							{
								Request["Account"] = Input.value;
								Input.setCustomValidity("");
							}
							else
							{
								Input.setCustomValidity("Account name cannot be empty.");
								Input.reportValidity();
								return;
							}
							break;

						case 1:
							if (Input.value.length > 0)
							{
								Request["PNr"] = Input.value;
								Input.setCustomValidity("");
							}
							else
							{
								Input.setCustomValidity("Personal number cannot be empty.");
								Input.reportValidity();
								return;
							}
							break;

						case 2:
							var CountryPattern = /[A-Z]{2}/;

							if (CountryPattern.test(Input.value))
							{
								Request["Country"] = Input.value;
								Input.setCustomValidity("");
							}
							else
							{
								Input.setCustomValidity("Enter a valid 2-character Country Code.");
								Input.reportValidity();
								return;
							}
							break;

						case 3:
							if (Input.checkValidity())
							{
								Request["Amount"] = parseFloat(Input.value);
								Input.setCustomValidity("");
							}
							else
							{
								Input.setCustomValidity("Enter a valid amount.");
								Input.reportValidity();
								return;
							}
							break;

						case 4:
							var CurrencyPattern = /[A-Z]{3}/;

							if (CurrencyPattern.test(Input.value))
							{
								Request["Currency"] = Input.value;
								Input.setCustomValidity("");
							}
							else
							{
								Input.setCustomValidity("Enter a valid 3-character Currency Code.");
								Input.reportValidity();
								return;
							}
							break;

						case 5:
							var DurationPattern = /^(-?)P(?=\d|T\d)(?:(\d+)Y)?(?:(\d+)M)?(?:(\d+)([DW]))?(?:T(?:(\d+)H)?(?:(\d+)M)?(?:(\d+(?:\.\d+)?)S)?)?$/;

							if (DurationPattern.test(Input.value))
							{
								Request["Period"] = Input.value;
								Input.setCustomValidity("");
							}
							else
							{
								Input.setCustomValidity("Enter a valid period (as a duration).");
								Input.reportValidity();
								return;
							}
							break;

						case 6:
							if (Input.checkValidity())
							{
								Request["PeriodInterest"] = parseFloat(Input.value);
								Input.setCustomValidity("");
							}
							else
							{
								Input.setCustomValidity("Enter a valid interest rate.");
								Input.reportValidity();
								return;
							}
							break;

						case 7:
							if (Input.checkValidity())
							{
								Request["MaxInstallments"] = parseInt(Input.value);
								Input.setCustomValidity("");
							}
							else
							{
								Input.setCustomValidity("Enter a valid maximum number of installments.");
								Input.reportValidity();
								return;
							}
							break;
					}
				}
			}

			Loop = Loop.nextSibling;
		}

		xhttp.open("POST", "Api/EditAccount.ws", true);
		xhttp.setRequestHeader("Accept", "application/json");
		xhttp.setRequestHeader("Content-Type", "application/json");
		xhttp.send(JSON.stringify(Request));
	}
	catch (e)
	{
		console.log(e);
		window.alert("Input fields contain errors.");
	}
}

function GetAccountDebt(Td, Account)
{
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
				Td.innerText = xhttp.responseText;
			else
				ShowError(xhttp);
		};
	}

	try
	{
		var Request = {
			"Account": Account
		};

		xhttp.open("POST", "Api/GetAccountDebt.ws", true);
		xhttp.setRequestHeader("Accept", "application/json");
		xhttp.setRequestHeader("Content-Type", "application/json");
		xhttp.send(JSON.stringify(Request));
	}
	catch (e)
	{
		console.log(e);
	}
}

function EditAccount(Control)
{
	var ObjectId = Control.getAttribute("data-objectid");
	var Td = Control.parentNode;
	var Tr = Td.parentNode;
	var Loop = Tr.firstChild;
	var i = 0;

	while (Loop !== null)
	{
		if (Loop.tagName === "TD")
		{
			switch (i++)
			{
				case 0:
					var Input = document.createElement("INPUT");
					Input.setAttribute("type", "text");
					Input.setAttribute("name", "Account");
					Input.setAttribute("required", "required");
					Input.setAttribute("data-org", Loop.innerText);
					Input.value = Loop.innerText;
					Loop.innerHTML = "";
					Loop.appendChild(Input);
					Input.focus();
					break;

				case 1:
					Input = document.createElement("INPUT");
					Input.setAttribute("type", "text");
					Input.setAttribute("name", "PNr");
					Input.setAttribute("required", "required");
					Input.setAttribute("data-org", Loop.innerText);
					Input.value = Loop.innerText;
					Loop.innerHTML = "";
					Loop.appendChild(Input);
					break;

				case 2:
					Input = document.createElement("INPUT");
					Input.setAttribute("type", "text");
					Input.setAttribute("name", "Country");
					Input.setAttribute("pattern", "^[A-Z]{2}$");
					Input.setAttribute("required", "required");
					Input.setAttribute("data-org", Loop.innerText);
					Input.value = Loop.innerText;
					Loop.innerHTML = "";
					Loop.appendChild(Input);
					break;

				case 3:
					Input = document.createElement("INPUT");
					Input.setAttribute("type", "number");
					Input.setAttribute("name", "Amount");
					Input.setAttribute("min", "0");
					Input.setAttribute("required", "required");
					Input.setAttribute("data-org", Loop.innerText);
					Input.value = Loop.innerText;
					Loop.innerHTML = "";
					Loop.appendChild(Input);
					break;

				case 4:
					Input = document.createElement("INPUT");
					Input.setAttribute("type", "text");
					Input.setAttribute("name", "Currency");
					Input.setAttribute("pattern", "^[A-Z]{3}$");
					Input.setAttribute("required", "required");
					Input.setAttribute("data-org", Loop.innerText);
					Input.value = Loop.innerText;
					Loop.innerHTML = "";
					Loop.appendChild(Input);
					break;

				case 5:
					Input = document.createElement("INPUT");
					Input.setAttribute("type", "text");
					Input.setAttribute("name", "Period");
					Input.setAttribute("pattern", "^(-?)P(?=\\d|T\\d)(?:(\\d+)Y)?(?:(\\d+)M)?(?:(\\d+)([DW]))?(?:T(?:(\\d+)H)?(?:(\\d+)M)?(?:(\\d+(?:\\.\\d+)?)S)?)?$");
					Input.setAttribute("required", "required");
					Input.setAttribute("data-org", Loop.innerText);
					Input.value = Loop.innerText;
					Loop.innerHTML = "";
					Loop.appendChild(Input);
					break;

				case 6:
					Input = document.createElement("INPUT");
					Input.setAttribute("type", "number");
					Input.setAttribute("name", "Interest");
					Input.setAttribute("min", "0");
					Input.setAttribute("required", "required");
					Input.setAttribute("data-org", Loop.innerText);
					Input.value = Loop.innerText.replace(" %", "");
					Loop.innerHTML = "";
					Loop.appendChild(Input);
					break;

				case 7:
					Input = document.createElement("INPUT");
					Input.setAttribute("type", "number");
					Input.setAttribute("name", "MaxInstallments");
					Input.setAttribute("min", "1");
					Input.setAttribute("max", "12");
					Input.setAttribute("required", "required");
					Input.setAttribute("data-org", Loop.innerText);
					Input.value = Loop.innerText;
					Loop.innerHTML = "";
					Loop.appendChild(Input);
					break;

				case 8:
					break;

				case 9:
					var Button = document.createElement("BUTTON");
					Button.setAttribute("type", "button");
					Button.setAttribute("class", "posButtonSm");
					Button.setAttribute("onclick", "OkAccount(this)");
					Button.setAttribute("data-objectid", ObjectId);
					Button.innerText = "OK";
					Loop.innerHTML = "";
					Loop.appendChild(Button);
					break;

				case 10:
					Button = document.createElement("BUTTON");
					Button.setAttribute("type", "button");
					Button.setAttribute("class", "negButtonSm");
					Button.setAttribute("onclick", "CancelAccount(this)");
					Button.setAttribute("data-objectid", ObjectId);
					Button.innerText = "Cancel";
					Loop.innerHTML = "";
					Loop.appendChild(Button);
					break;
			}
		}

		Loop = Loop.nextSibling;
	}
}

function CancelAccount(Control)
{
	var ObjectId = Control.getAttribute("data-objectid");
	var Td = Control.parentNode;
	var Tr = Td.parentNode;

	if (ObjectId)
	{
		var Loop = Tr.firstChild;
		var i = 0;

		while (Loop !== null)
		{
			if (Loop.tagName === "TD")
			{
				switch (i++)
				{
					case 0:
						var Input = FindChild(Loop, "INPUT");
						var Org;

						if (Input)
						{
							Org = Input.getAttribute("data-org");
							Loop.innerHTML = "";
							var Code = document.createElement("CODE");
							Code.innerText = Org;
							Loop.appendChild(Code);
						}
						break;

					default:
						var Input = FindChild(Loop, "INPUT");
						if (Input)
						{
							Org = Input.getAttribute("data-org");
							Loop.innerText = Org;
						}
						break;

					case 8:
						break;

					case 9:
						Loop.innerHTML = "";
						var Button = document.createElement("BUTTON");
						Button.setAttribute("type", "button");
						Button.setAttribute("class", "posButtonSm");
						Button.setAttribute("data-objectid", ObjectId);
						Button.setAttribute("onclick", "EditAccount(this)");
						Button.innerText = "Edit";
						Loop.appendChild(Button);
						break;

					case 10:
						Loop.innerHTML = "";
						Button = document.createElement("BUTTON");
						Button.setAttribute("type", "button");
						Button.setAttribute("class", "negButtonSm");
						Button.setAttribute("data-objectid", ObjectId);
						Button.setAttribute("onclick", "DeleteAccount(this)");
						Button.innerText = "Delete";
						Loop.appendChild(Button);
						break;
				}
			}

			Loop = Loop.nextSibling;
		}
	}
	else
	{
		var TBody = Tr.parentNode;
		TBody.removeChild(Tr);
	}
}

function DeleteAccount(Control)
{
	if (!window.confirm("Are you sure you want to delete these account settings?"))
		return;

	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
			{
				var Td = Control.parentNode;
				var Tr = Td.parentNode;
				var TBody = Tr.parentNode;
				TBody.removeChild(Tr);
			}
			else
				ShowError(xhttp);
		};
	}

	try
	{
		var Request = {
			"ObjectId": Control.getAttribute("data-objectid")
		};

		xhttp.open("POST", "Api/DeleteAccount.ws", true);
		xhttp.setRequestHeader("Accept", "application/json");
		xhttp.setRequestHeader("Content-Type", "application/json");
		xhttp.send(JSON.stringify(Request));
	}
	catch (e)
	{
		console.log(e);
	}
}

function AddOrganization()
{
	var Table = FindTable("OrganizationAuthorizations");
	var Body = FindChild(Table, "TBODY");
	var Tr = document.createElement("TR");
	Body.appendChild(Tr);

	var Td = document.createElement("TD");
	Tr.appendChild(Td);

	var Input = document.createElement("INPUT");
	Input.setAttribute("type", "text");
	Input.setAttribute("name", "OrganizationName");
	Input.setAttribute("required", "required");
	Td.appendChild(Input);
	Input.focus();

	Td = document.createElement("TD");
	Tr.appendChild(Td);

	Input = document.createElement("INPUT");
	Input.setAttribute("type", "text");
	Input.setAttribute("name", "OrganizationNumber");
	Input.setAttribute("required", "required");
	Td.appendChild(Input);

	Td = document.createElement("TD");
	Tr.appendChild(Td);

	Input = document.createElement("INPUT");
	Input.setAttribute("type", "text");
	Input.setAttribute("name", "OrganizationCountry");
	Input.setAttribute("pattern", "^[A-Z]{2}$");
	Input.setAttribute("required", "required");
	Td.appendChild(Input);

	Td = document.createElement("TD");
	Tr.appendChild(Td);

	Input = document.createElement("INPUT");
	Input.setAttribute("type", "text");
	Input.setAttribute("name", "PersonalNumbers");
	Input.setAttribute("required", "required");
	Td.appendChild(Input);

	Td = document.createElement("TD");
	Tr.appendChild(Td);

	Input = document.createElement("INPUT");
	Input.setAttribute("type", "text");
	Input.setAttribute("name", "PersonalCountries");
	Input.setAttribute("pattern", "^[A-Z]{2}(,[A-Z]{2})*$");
	Input.setAttribute("required", "required");
	Td.appendChild(Input);

	Td = document.createElement("TD");
	Td.setAttribute("style", "text-align:right");
	Tr.appendChild(Td);

	Input = document.createElement("INPUT");
	Input.setAttribute("type", "number");
	Input.setAttribute("name", "Amount");
	Input.setAttribute("min", "0");
	Input.setAttribute("value", document.getElementById("DefaultOrganizationalLimit").value);
	Input.setAttribute("required", "required");
	Td.appendChild(Input);

	Td = document.createElement("TD");
	Tr.appendChild(Td);

	Input = document.createElement("INPUT");
	Input.setAttribute("type", "text");
	Input.setAttribute("name", "Currency");
	Input.setAttribute("pattern", "^[A-Z]{3}$");
	Input.setAttribute("value", document.getElementById("DefaultCurrency").value);
	Input.setAttribute("required", "required");
	Td.appendChild(Input);

	Td = document.createElement("TD");
	Td.setAttribute("style", "text-align:right");
	Tr.appendChild(Td);

	Input = document.createElement("INPUT");
	Input.setAttribute("type", "text");
	Input.setAttribute("name", "Period");
	Input.setAttribute("pattern", "^(-?)P(?=\\d|T\\d)(?:(\\d+)Y)?(?:(\\d+)M)?(?:(\\d+)([DW]))?(?:T(?:(\\d+)H)?(?:(\\d+)M)?(?:(\\d+(?:\\.\\d+)?)S)?)?$");
	Input.setAttribute("value", document.getElementById("DefaultPeriod").value);
	Input.setAttribute("required", "required");
	Td.appendChild(Input);

	Td = document.createElement("TD");
	Td.setAttribute("style", "text-align:right");
	Tr.appendChild(Td);

	Input = document.createElement("INPUT");
	Input.setAttribute("type", "number");
	Input.setAttribute("name", "Interest");
	Input.setAttribute("min", "0");
	Input.setAttribute("value", document.getElementById("DefaultPeriodInterest").value);
	Input.setAttribute("required", "required");
	Td.appendChild(Input);

	Td = document.createElement("TD");
	Td.setAttribute("style", "text-align:right");
	Tr.appendChild(Td);

	Input = document.createElement("INPUT");
	Input.setAttribute("type", "number");
	Input.setAttribute("name", "MaxInstallments");
	Input.setAttribute("min", "1");
	Input.setAttribute("max", "12");
	Input.setAttribute("value", document.getElementById("DefaultMaxInstallments").value);
	Input.setAttribute("required", "required");
	Td.appendChild(Input);

	Td = document.createElement("TD");
	Td.setAttribute("style", "text-align:right");
	Tr.appendChild(Td);

	Td = document.createElement("TD");
	Tr.appendChild(Td);

	var Button = document.createElement("BUTTON");
	Button.setAttribute("type", "button");
	Button.setAttribute("class", "posButtonSm");
	Button.setAttribute("onclick", "OkOrganization(this)");
	Button.innerText = "Add";
	Td.appendChild(Button);

	Td = document.createElement("TD");
	Tr.appendChild(Td);

	Button = document.createElement("BUTTON");
	Button.setAttribute("type", "button");
	Button.setAttribute("class", "negButtonSm");
	Button.setAttribute("onclick", "CancelOrganization(this)");
	Button.innerText = "Cancel";
	Td.appendChild(Button);
}

function OkOrganization(Control)
{
	var Td = Control.parentNode;
	var Tr = Td.parentNode;
	var Loop;
	var i;

	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
			{
				try
				{
					var Data = JSON.parse(xhttp.responseText);

					Loop = Tr.firstChild;
					i = 0;

					while (Loop !== null)
					{
						if (Loop.tagName === "TD")
						{
							switch (i++)
							{
								case 0:
									Loop.innerText = Data.OrganizationName;
									break;

								case 1:
									Loop.innerText = Data.OrganizationNumber;
									break;

								case 2:
									Loop.innerText = Data.OrganizationCountry;
									break;

								case 3:
									Loop.innerText = Data.PersonalNumbers;
									break;

								case 4:
									Loop.innerText = Data.PersonalCountries;
									break;

								case 5:
									Loop.innerText = Data.MaxCredit;
									break;

								case 6:
									Loop.innerText = Data.Currency;
									break;

								case 7:
									Loop.innerText = Data.Period;
									break;

								case 8:
									Loop.innerText = Data.PeriodInterest + " %";
									break;

								case 9:
									Loop.innerText = Data.MaxInstallments;
									break;

								case 10:
									GetOrganizationDebt(Loop, Data.OrganizationName);
									break;

								case 11:
									Loop.innerHTML = "";
									var Button = document.createElement("BUTTON");
									Button.setAttribute("type", "button");
									Button.setAttribute("class", "posButtonSm");
									Button.setAttribute("data-objectid", Data.ObjectId);
									Button.setAttribute("onclick", "EditOrganization(this)");
									Button.innerText = "Edit";
									Loop.appendChild(Button);
									break;

								case 12:
									Loop.innerHTML = "";
									Button = document.createElement("BUTTON");
									Button.setAttribute("type", "button");
									Button.setAttribute("class", "negButtonSm");
									Button.setAttribute("data-objectid", Data.ObjectId);
									Button.setAttribute("onclick", "DeleteOrganization(this)");
									Button.innerText = "Delete";
									Loop.appendChild(Button);
									break;
							}
						}

						Loop = Loop.nextSibling;
					}
				}
				catch (e)
				{
					console.log(e);
					console.log(xhttp.responseText);
				}
			}
			else
				ShowError(xhttp);
		};
	}

	try
	{
		Loop = Tr.firstChild;
		i = 0;
		var Request = {
			"ObjectId": Control.getAttribute("data-objectid")
		};

		while (Loop !== null)
		{
			if (Loop.tagName === "TD")
			{
				var Input = FindChild(Loop, "INPUT");

				if (Input)
				{
					switch (i++)
					{
						case 0:
							if (Input.value.length > 0)
							{
								Request["OrganizationName"] = Input.value;
								Input.setCustomValidity("");
							}
							else
							{
								Input.setCustomValidity("Organization name cannot be empty.");
								Input.reportValidity();
								return;
							}
							break;

						case 1:
							if (Input.value.length > 0)
							{
								Request["OrganizationNumber"] = Input.value;
								Input.setCustomValidity("");
							}
							else
							{
								Input.setCustomValidity("Organization number cannot be empty.");
								Input.reportValidity();
								return;
							}
							break;

						case 2:
							var CountryPattern = /[A-Z]{2}/;

							if (CountryPattern.test(Input.value))
							{
								Request["OrganizationCountry"] = Input.value;
								Input.setCustomValidity("");
							}
							else
							{
								Input.setCustomValidity("Enter a valid 2-character Country Code.");
								Input.reportValidity();
								return;
							}
							break;

						case 3:
							if (Input.value.length > 0)
							{
								Request["PersonalNumbers"] = Input.value;
								Input.setCustomValidity("");
							}
							else
							{
								Input.setCustomValidity("Enter a sequence of personal numbers (delimited by commas).");
								Input.reportValidity();
								return;
							}
							break;

						case 4:
							CountryPattern = /[A-Z]{2}(,[A-Z]{2})*/;

							if (CountryPattern.test(Input.value))
							{
								Request["PersonalCountries"] = Input.value;
								Input.setCustomValidity("");
							}
							else
							{
								Input.setCustomValidity("Enter a sequence of 2-character Country Codes (delimited by commas).");
								Input.reportValidity();
								return;
							}
							break;

						case 5:
							if (Input.checkValidity())
							{
								Request["Amount"] = parseFloat(Input.value);
								Input.setCustomValidity("");
							}
							else
							{
								Input.setCustomValidity("Enter a valid amount.");
								Input.reportValidity();
								return;
							}
							break;

						case 6:
							var CurrencyPattern = /[A-Z]{3}/;

							if (CurrencyPattern.test(Input.value))
							{
								Request["Currency"] = Input.value;
								Input.setCustomValidity("");
							}
							else
							{
								Input.setCustomValidity("Enter a valid 3-character Currency Code.");
								Input.reportValidity();
								return;
							}
							break;

						case 7:
							var DurationPattern = /^(-?)P(?=\d|T\d)(?:(\d+)Y)?(?:(\d+)M)?(?:(\d+)([DW]))?(?:T(?:(\d+)H)?(?:(\d+)M)?(?:(\d+(?:\.\d+)?)S)?)?$/;

							if (DurationPattern.test(Input.value))
							{
								Request["Period"] = Input.value;
								Input.setCustomValidity("");
							}
							else
							{
								Input.setCustomValidity("Enter a valid period (as a duration).");
								Input.reportValidity();
								return;
							}
							break;

						case 8:
							if (Input.checkValidity())
							{
								Request["PeriodInterest"] = parseFloat(Input.value);
								Input.setCustomValidity("");
							}
							else
							{
								Input.setCustomValidity("Enter a valid interest rate.");
								Input.reportValidity();
								return;
							}
							break;

						case 9:
							if (Input.checkValidity())
							{
								Request["MaxInstallments"] = parseInt(Input.value);
								Input.setCustomValidity("");
							}
							else
							{
								Input.setCustomValidity("Enter a valid maximum number of installments.");
								Input.reportValidity();
								return;
							}
							break;
					}
				}
			}

			Loop = Loop.nextSibling;
		}

		xhttp.open("POST", "Api/EditOrganization.ws", true);
		xhttp.setRequestHeader("Accept", "application/json");
		xhttp.setRequestHeader("Content-Type", "application/json");
		xhttp.send(JSON.stringify(Request));
	}
	catch (e)
	{
		console.log(e);
		window.alert("Input fields contain errors.");
	}
}

function GetOrganizationDebt(Td, OrganizationName)
{
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
				Td.innerText = xhttp.responseText;
			else
				ShowError(xhttp);
		};
	}

	try
	{
		var Request = {
			"OrganizationName": OrganizationName
		};

		xhttp.open("POST", "Api/GetOrganizationDebt.ws", true);
		xhttp.setRequestHeader("Accept", "application/json");
		xhttp.setRequestHeader("Content-Type", "application/json");
		xhttp.send(JSON.stringify(Request));
	}
	catch (e)
	{
		console.log(e);
	}
}

function EditOrganization(Control)
{
	var ObjectId = Control.getAttribute("data-objectid");
	var Td = Control.parentNode;
	var Tr = Td.parentNode;
	var Loop = Tr.firstChild;
	var i = 0;

	while (Loop !== null)
	{
		if (Loop.tagName === "TD")
		{
			switch (i++)
			{
				case 0:
					var Input = document.createElement("INPUT");
					Input.setAttribute("type", "text");
					Input.setAttribute("name", "OrganizationName");
					Input.setAttribute("required", "required");
					Input.setAttribute("data-org", Loop.innerText);
					Input.value = Loop.innerText;
					Loop.innerHTML = "";
					Loop.appendChild(Input);
					Input.focus();
					break;

				case 1:
					Input = document.createElement("INPUT");
					Input.setAttribute("type", "text");
					Input.setAttribute("name", "OrganizationNumber");
					Input.setAttribute("required", "required");
					Input.setAttribute("data-org", Loop.innerText);
					Input.value = Loop.innerText;
					Loop.innerHTML = "";
					Loop.appendChild(Input);
					break;

				case 2:
					Input = document.createElement("INPUT");
					Input.setAttribute("type", "text");
					Input.setAttribute("name", "OrganizationCountry");
					Input.setAttribute("pattern", "^[A-Z]{2}$");
					Input.setAttribute("required", "required");
					Input.setAttribute("data-org", Loop.innerText);
					Input.value = Loop.innerText;
					Loop.innerHTML = "";
					Loop.appendChild(Input);
					break;

				case 3:
					Input = document.createElement("INPUT");
					Input.setAttribute("type", "text");
					Input.setAttribute("name", "PersonalNumbers");
					Input.setAttribute("required", "required");
					Input.setAttribute("data-org", Loop.innerText);
					Input.value = Loop.innerText;
					Loop.innerHTML = "";
					Loop.appendChild(Input);
					break;

				case 4:
					Input = document.createElement("INPUT");
					Input.setAttribute("type", "text");
					Input.setAttribute("name", "PersonalCountries");
					Input.setAttribute("required", "required");
					Input.setAttribute("data-org", Loop.innerText);
					Input.value = Loop.innerText;
					Loop.innerHTML = "";
					Loop.appendChild(Input);
					break;

				case 5:
					Input = document.createElement("INPUT");
					Input.setAttribute("type", "number");
					Input.setAttribute("name", "Amount");
					Input.setAttribute("min", "0");
					Input.setAttribute("required", "required");
					Input.setAttribute("data-org", Loop.innerText);
					Input.value = Loop.innerText;
					Loop.innerHTML = "";
					Loop.appendChild(Input);
					break;

				case 6:
					Input = document.createElement("INPUT");
					Input.setAttribute("type", "text");
					Input.setAttribute("name", "Currency");
					Input.setAttribute("pattern", "^[A-Z]{3}$");
					Input.setAttribute("required", "required");
					Input.setAttribute("data-org", Loop.innerText);
					Input.value = Loop.innerText;
					Loop.innerHTML = "";
					Loop.appendChild(Input);
					break;

				case 7:
					Input = document.createElement("INPUT");
					Input.setAttribute("type", "text");
					Input.setAttribute("name", "Period");
					Input.setAttribute("pattern", "^(-?)P(?=\\d|T\\d)(?:(\\d+)Y)?(?:(\\d+)M)?(?:(\\d+)([DW]))?(?:T(?:(\\d+)H)?(?:(\\d+)M)?(?:(\\d+(?:\\.\\d+)?)S)?)?$");
					Input.setAttribute("required", "required");
					Input.setAttribute("data-org", Loop.innerText);
					Input.value = Loop.innerText;
					Loop.innerHTML = "";
					Loop.appendChild(Input);
					break;

				case 8:
					Input = document.createElement("INPUT");
					Input.setAttribute("type", "number");
					Input.setAttribute("name", "Interest");
					Input.setAttribute("min", "0");
					Input.setAttribute("required", "required");
					Input.setAttribute("data-org", Loop.innerText);
					Input.value = Loop.innerText.replace(" %", "");
					Loop.innerHTML = "";
					Loop.appendChild(Input);
					break;

				case 9:
					Input = document.createElement("INPUT");
					Input.setAttribute("type", "number");
					Input.setAttribute("name", "MaxInstallments");
					Input.setAttribute("min", "1");
					Input.setAttribute("max", "12");
					Input.setAttribute("required", "required");
					Input.setAttribute("data-org", Loop.innerText);
					Input.value = Loop.innerText;
					Loop.innerHTML = "";
					Loop.appendChild(Input);
					break;

				case 10:
					break;

				case 11:
					var Button = document.createElement("BUTTON");
					Button.setAttribute("type", "button");
					Button.setAttribute("class", "posButtonSm");
					Button.setAttribute("onclick", "OkOrganization(this)");
					Button.setAttribute("data-objectid", ObjectId);
					Button.innerText = "OK";
					Loop.innerHTML = "";
					Loop.appendChild(Button);
					break;

				case 12:
					Button = document.createElement("BUTTON");
					Button.setAttribute("type", "button");
					Button.setAttribute("class", "negButtonSm");
					Button.setAttribute("onclick", "CancelOrganization(this)");
					Button.setAttribute("data-objectid", ObjectId);
					Button.innerText = "Cancel";
					Loop.innerHTML = "";
					Loop.appendChild(Button);
					break;
			}
		}

		Loop = Loop.nextSibling;
	}
}

function CancelOrganization(Control)
{
	var ObjectId = Control.getAttribute("data-objectid");
	var Td = Control.parentNode;
	var Tr = Td.parentNode;

	if (ObjectId)
	{
		var Loop = Tr.firstChild;
		var i = 0;

		while (Loop !== null)
		{
			if (Loop.tagName === "TD")
			{
				switch (i++)
				{
					default:
						var Input = FindChild(Loop, "INPUT");
						if (Input)
						{
							Org = Input.getAttribute("data-org");
							Loop.innerText = Org;
						}
						break;

					case 10:
						break;

					case 11:
						Loop.innerHTML = "";
						var Button = document.createElement("BUTTON");
						Button.setAttribute("type", "button");
						Button.setAttribute("class", "posButtonSm");
						Button.setAttribute("data-objectid", ObjectId);
						Button.setAttribute("onclick", "EditOrganization(this)");
						Button.innerText = "Edit";
						Loop.appendChild(Button);
						break;

					case 12:
						Loop.innerHTML = "";
						Button = document.createElement("BUTTON");
						Button.setAttribute("type", "button");
						Button.setAttribute("class", "negButtonSm");
						Button.setAttribute("data-objectid", ObjectId);
						Button.setAttribute("onclick", "DeleteOrganization(this)");
						Button.innerText = "Delete";
						Loop.appendChild(Button);
						break;
				}
			}

			Loop = Loop.nextSibling;
		}
	}
	else
	{
		var TBody = Tr.parentNode;
		TBody.removeChild(Tr);
	}
}

function DeleteOrganization(Control)
{
	if (!window.confirm("Are you sure you want to delete these organization settings?"))
		return;

	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
			{
				var Td = Control.parentNode;
				var Tr = Td.parentNode;
				var TBody = Tr.parentNode;
				TBody.removeChild(Tr);
			}
			else
				ShowError(xhttp);
		};
	}

	try
	{
		var Request = {
			"ObjectId": Control.getAttribute("data-objectid")
		};

		xhttp.open("POST", "Api/DeleteOrganization.ws", true);
		xhttp.setRequestHeader("Accept", "application/json");
		xhttp.setRequestHeader("Content-Type", "application/json");
		xhttp.send(JSON.stringify(Request));
	}
	catch (e)
	{
		console.log(e);
	}
}

function FindTable(ParentId)
{
	var Parent = document.getElementById(ParentId);
	return FindChild(Parent, "TABLE");
}

function FindChild(Parent, TagName)
{
	var Loop = Parent.firstChild;

	while (Loop && Loop.tagName !== TagName)
		Loop = Loop.nextSibling;

	return Loop;
}

function OpenUrl(Url)
{
	window.open(Url, "_blank");
}

function Close()
{
	window.close();
}

function LoadMore(Button, Offset, MaxCount, Type)
{
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4)
		{
			Button.removeAttribute("data-scroll");

			if (xhttp.status === 200)
			{
				var Response = JSON.parse(xhttp.responseText);
				var i, c = Response.length;

				var Loop = Button.parentNode.firstChild;
				while (Loop.tagName !== "TABLE")
					Loop = Loop.nextSibling;

				var Table = Loop;
				Loop = Loop.firstChild;
				while (Loop)
				{
					if (Loop.tagName == "TBODY")
					{
						Table = Loop;
						Loop = Loop.firstChild;
					}
					else
						Loop = Loop.nextSibling;
				}

				for (i = 0; i < c; i++)
				{
					var Invoice = Response[i];

					var Tr = document.createElement("TR");
					Table.appendChild(Tr);

					var Td = document.createElement("TD");
					Tr.appendChild(Td);
					Td.setAttribute("style", "text-align:right");
					Td.innerText = Invoice.invoiceNumber;

					Td = document.createElement("TD");
					Td.setAttribute("style", "text-align:left");
					Tr.appendChild(Td);
					Td.innerText = Invoice.name;

					Td = document.createElement("TD");
					Td.setAttribute("style", "text-align:center");
					Tr.appendChild(Td);
					Td.innerText = Invoice.nr;

					if (Type == "Cancelled")
					{
						Td = document.createElement("TD");
						Td.setAttribute("style", "text-align:right");
						Tr.appendChild(Td);
						Td.innerText = Invoice.amountPaid;
					}
					else if (Type == "Pending")
					{
						Td = document.createElement("TD");
						Td.setAttribute("style", "text-align:right");
						Tr.appendChild(Td);
						Td.innerText = Invoice.total;

						Td = document.createElement("TD");
						Td.setAttribute("style", "text-align:right");
						Tr.appendChild(Td);
						Td.innerText = Invoice.left;
					}

					Td = document.createElement("TD");
					Td.setAttribute("style", "text-align:left");
					Tr.appendChild(Td);
					Td.innerText = Invoice.currency;

					Td = document.createElement("TD");
					Td.setAttribute("style", "text-align:center");
					Tr.appendChild(Td);
					Td.innerText = Invoice.created;

					if (Type == "Cancelled")
					{
						Td = document.createElement("TD");
						Td.setAttribute("style", "text-align:center");
						Tr.appendChild(Td);
						Td.innerText = Invoice.paid;
					}
					else if (Type == "Pending")
					{
						Td = document.createElement("TD");
						Td.setAttribute("style", "text-align:center");
						Tr.appendChild(Td);
						Td.innerText = Invoice.due;
					}

					Td = document.createElement("TD");
					Td.setAttribute("style", "text-align:right");
					Tr.appendChild(Td);
					Td.innerText = Invoice.nrReminders;

					Td = document.createElement("TD");
					Td.setAttribute("style", "text-align:center");
					Tr.appendChild(Td);
					Td.innerText = Invoice.installment + "/" + Invoice.nrInstallments;

					Td = document.createElement("TD");
					Td.setAttribute("style", "text-align:center");
					Tr.appendChild(Td);

					if (Invoice.contractId)
					{
						var A = document.createElement("A");
						A.setAttribute("href", "/Contract.md?ID=" + Invoice.contractId);
						A.setAttribute("target", "_blank");
						A.innerText = "Contract";
						Td.appendChild(A);
					}
				}

				if (c < MaxCount)
					Button.parentNode.removeChild(Button);
				else
				{
					Button.setAttribute("onclick", "LoadMore(this," + (Offset + MaxCount) +
						"," + MaxCount + ",\"" + Type + "\")");
				}
			}
		}
	};

	Button.setAttribute("data-scroll", "x");
	xhttp.open("POST", "Api/LoadMore"+Type+".ws", true);
	xhttp.setRequestHeader("Content-Type", "application/json");
	xhttp.setRequestHeader("Accept", "application/json");
	xhttp.send(JSON.stringify(
		{
			"offset": Offset,
			"maxCount": MaxCount
		}
	));
}

window.onscroll = function ()
{
	var Button = document.getElementById("LoadMoreButton");

	if (Button)
	{
		var Rect = Button.getBoundingClientRect();
		if (Rect.top <= window.innerHeight * 2)
		{
			var Scroll = Button.getAttribute("data-scroll");
			if (Scroll !== "x")
				Button.click();
		}
	}
}
