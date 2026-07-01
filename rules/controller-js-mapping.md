# Controller <-> JS mapping

Goal: for a given controller, find every frontend call site that actually invokes it, and extract
exactly what that call site sends - not what the controller's C# signature suggests it accepts.
The frontend call site is the evidence; the controller signature is only where you compare it.

## Step 1 - find the call sites

Search the JS files under `scriptRoot` for calls that could hit `/<Controller>/<Action>`. Look for
all of these patterns (a project may use several at once):

* **`$http`**: `$http.post(url, data)`, `$http.get(url, { params })`, `$http({ method, url, data })`.
* **`$.ajax` / jQuery**: `$.ajax({ url, type, data, contentType })`, `$.post(url, data)`, `$.get(url)`.
* **Custom API wrapper services**: a project-local service (often named something like `apiService`,
  `httpService`, `dataService`) that wraps `$http`/`$.ajax` - find its definition first, then find
  every controller-level call *through* it, since the wrapper may set a default `Content-Type` or
  serialize the body in a way that isn't visible at the call site itself.
* **Angular `$resource` / service methods**: declarative REST bindings where the URL/verb is
  configured once and called via a method name elsewhere - trace back to that configuration.

For each call site, record: the literal URL (resolve any string concatenation/template), the HTTP
verb, and the exact variable/object passed as the body or params.

## Step 2 - identify content type

Determine what content type the request actually goes out as - don't assume JSON by default:

| Evidence in the JS | Content type |
|---|---|
| Body is a plain JS object/array passed to `$http.post`/`$.ajax` with default settings, or `JSON.stringify`d explicitly | `application/json` |
| Body built via `$.param(obj)`, a `FormData`-less key/value string, or `$http` with `Content-Type: application/x-www-form-urlencoded` set explicitly | `application/x-www-form-urlencoded` |
| Body is a `FormData` object (often for file upload) | `multipart/form-data` |
| No body; all data passed as `params`/query string on a GET | query string only |

If the wrapper service sets a default (many AngularJS `$http` setups default POST bodies to JSON
unless overridden), that default applies unless the call site overrides it - check the wrapper's
defaults, don't assume.

## Step 3 - identify the payload shape actually sent

Extract the literal field names and types as constructed in JS just before the call, e.g.:

```js
var payload = { SupplierID: vm.supplierId, OrderType: vm.orderType, listModel: [] };
$http.post('/Suppliers/AddProducts', payload);
```

Note anything a static read of the C# DTO wouldn't tell you:
* a field built from a form input bound with `ng-model` to a **string** scope property, even though
  the DTO field is numeric/boolean/enum on the C# side (this is exactly the class of risk
  `CompatibilityScenarios.NumberAsString` / `BoolAsString` / `EnumAsString` exist for),
  * a field that's conditionally omitted (`if (x) payload.Foo = x;`) - evidence for
  `CompatibilityScenarios.MissingOptionalField`,
* a field the JS sends but the DTO doesn't declare - evidence for
  `CompatibilityScenarios.ExtraUnknownField`,
* an array that can legitimately be `null` vs `[]` depending on UI state - evidence for
  `NullCollection`/`EmptyCollection`.

## Step 4 - compare JS payload fields with the MVC action's parameters/DTOs

Open the controller action and its parameter type(s). For each JS-sent field, find the matching C#
property and note:

* its declared CLR type (is it `int`, `int?`, `bool`, an `enum`, `DateTime`, `decimal`, ...?) versus
  what JS actually sends (string vs number vs bool vs omitted),
* whether the property is required or has a default,
* whether the action itself does any pre-forwarding transformation (see `scenario-authoring.md` -
  this affects whether a binding risk is even observable at the forwarding seam).

The output of this step is the evidence list that `scenario-authoring.md` turns into actual
`CompatibilityCase`s - don't skip straight to writing cases without this comparison written down
somewhere in your working notes/coverage report.
