using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class iframe_sample : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e) {
        string merchant_id      = "XXXXXX";
        string merchant_key     = "YYYYYYYYYYYYYY";
        string merchant_salt    = "ZZZZZZZZZZZZZZ";
        string emailstr         = "XXXXXXXX";
        int payment_amountstr   = 999;
        string merchant_oid     = "";
        string user_namestr     = "";
        string user_addressstr  = "";
        string user_phonestr    = "";
        string merchant_ok_url      = "http://www.example.com/success.php";
        string merchant_fail_url    = "http://www.example.com/error.php";
        string user_ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
        if (user_ip == "" || user_ip == null){
            user_ip = Request.ServerVariables["REMOTE_ADDR"];
        }
        object[][] user_basket = {
            new object[] {"Sample Product 1", "18.00", 1},
            new object[] {"Sample Product 2", "33.25", 2},
            new object[] {"Sample Product 3", "45.42", 1},
            };

        string timeout_limit    = "30";
        string debug_on         = "1";
        string test_mode        = "0";
        string no_installment   = "0";
        string max_installment  = "0";
        string currency         = "TL";
        string lang             = "";

        NameValueCollection data = new NameValueCollection();
        data["merchant_id"] = merchant_id;
        data["user_ip"] = user_ip;
        data["merchant_oid"] = merchant_oid;
        data["email"] = emailstr;
        data["payment_amount"] = payment_amountstr.ToString();

        JavaScriptSerializer ser = new JavaScriptSerializer();
        string user_basket_json = ser.Serialize(user_basket);
        string user_basketstr = Convert.ToBase64String(Encoding.UTF8.GetBytes(user_basket_json));
        data["user_basket"] = user_basketstr;

        string Birlestir = string.Concat(merchant_id, user_ip, merchant_oid, emailstr, payment_amountstr.ToString(), user_basketstr, no_installment, max_installment, currency, test_mode, merchant_salt);
        HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(merchant_key));
        byte[] b = hmac.ComputeHash(Encoding.UTF8.GetBytes(Birlestir));
        data["paytr_token"] = Convert.ToBase64String(b);

        data["debug_on"] = debug_on;
        data["test_mode"] = test_mode;
        data["no_installment"] = no_installment;
        data["max_installment"] = max_installment;
        data["user_name"] = user_namestr;
        data["user_address"] = user_addressstr;
        data["user_phone"] = user_phonestr;
        data["merchant_ok_url"] = merchant_ok_url;
        data["merchant_fail_url"] = merchant_fail_url;
        data["timeout_limit"] = timeout_limit;
        data["currency"] = currency;
        data["lang"] = lang;

        using (WebClient client = new WebClient()) {
            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            byte[] result = client.UploadValues("https://www.paytr.com/odeme/api/get-token", "POST", data);
            string ResultAuthTicket = Encoding.UTF8.GetString(result);
            dynamic json = JValue.Parse(ResultAuthTicket);

            if (json.status == "success") {
                paytriframe.Src = "https://www.paytr.com/odeme/guvenli/" + json.token + "";
                paytriframe.Visible = true;
            }else{
                Response.Write("PAYTR IFRAME failed. reason:" + json.reason + "");
            }
        }
    }
}
