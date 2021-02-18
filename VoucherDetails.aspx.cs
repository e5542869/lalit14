using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Security;
using System.Globalization;

public partial class VoucherDetails : System.Web.UI.Page
{
    private int TypeId;

    SqlConnection con = new SqlConnection();
    protected void Page_Load(object sender, EventArgs e)
    {
        //if (User.Identity.IsAuthenticated)
        //{

        //}
        //else
        //{

        //    FormsAuthentication.RedirectToLoginPage();
        //}

        if (Session["Login"]!= null)
        {
            con.ConnectionString = ConfigurationManager.ConnectionStrings["cn"].ConnectionString;
            if (con.State == ConnectionState.Closed)
                con.Open();
        }
        else {
            Response.Redirect("Login.aspx");
        }
    }

    protected void GetTypeId()
    {
       if (ddlVoucherType.SelectedValue == "Contra")
            TypeId = 3;
        else if (ddlVoucherType.SelectedValue == "Payment")
            TypeId = 4;
        else if (ddlVoucherType.SelectedValue == "Receipt")
            TypeId = 5;
        else if (ddlVoucherType.SelectedValue == "Journal")
            TypeId = 6;
        else if (ddlVoucherType.SelectedValue == "Rectification Entry")
            TypeId = 29;

    }

    protected string GetTypeName(int id)
    {
        if (id == 3)
            return "Contra";
        else if (id == 4)
            return "Payment";
        else if (id == 5)
            return "Receipt";
        else if (id == 6)
            return "Journal";
        else if (id == 29)
            return "Rectification Entry";
        else
            return "";
    }
    protected void btnSearch_Click(object sender, EventArgs e)
    {
        GetTypeId();
        SqlCommand cmd = new SqlCommand();
        cmd.CommandText = "DisplayTransaction";
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Connection = con;
        cmd.Parameters.Add("@VoucherTypeId", SqlDbType.Int).Value = TypeId;
        cmd.Parameters.Add("@VoucherNo", SqlDbType.VarChar, 100).Value = txtVoucherNumber.Text;
        SqlDataReader dr = cmd.ExecuteReader();
        DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Voucher Type");
            dataTable.Columns.Add("Voucher No");
            dataTable.Columns.Add("Transaction Date");

        while (dr.Read())
        {
            txtTransactionDate.Text = Convert.ToDateTime(dr["TransactionDate"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
             DataRow row = dataTable.NewRow();
            ViewState["TransactionMasterId"]= dr["TransactionMasterId"];
            string voucherType=GetTypeName( Convert.ToInt32(dr["VoucherTypeId"]));
            row["Voucher Type"] = voucherType;
            row["Voucher No"] = dr["VoucherNo"];
            row["Transaction Date"] = dr["TransactionDate"];
            dataTable.Rows.Add(row);
            // txtTransactionDate.Text = dt.ToShortDateString();
        }
        grdTransaction.DataSource = dataTable;
       
        grdTransaction.DataBind();
        dr.Close();
        cmd.Dispose();

    }

    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        GetTypeId();
        SqlCommand cmd = new SqlCommand();
        cmd.CommandText = "UpdateTransaction";
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Connection = con;
        string voucherNo= txtVoucherNumber.Text;
        string voucherFirstString= voucherNo.Substring(0, voucherNo.IndexOf("/"));
        string voucherLastString = voucherNo.Substring(voucherNo.LastIndexOf("/")+1);
        //string dt = txtTransactionDate.Text;
        DateTime dt = Convert.ToDateTime(txtTransactionDate.Text);
        string date = dt.ToString("d/M/yyyy", CultureInfo.InvariantCulture);
        string dateToReplace = date.Replace("/",".");
        object[] array = { voucherFirstString, dateToReplace,voucherLastString };
        string replaceVoucherNo = string.Join("/", array);
        cmd.Parameters.Add("@VoucherTypeId", SqlDbType.Int).Value = TypeId;
        cmd.Parameters.Add("@VoucherNo", SqlDbType.VarChar, 100).Value = txtVoucherNumber.Text;
        cmd.Parameters.Add("@TransactionDate", SqlDbType.DateTime).Value = txtTransactionDate.Text;
        cmd.Parameters.Add("@ReplaceVoucherNo", SqlDbType.VarChar, 100).Value = replaceVoucherNo;
        cmd.ExecuteNonQuery();
        cmd.Dispose();

        ddlVoucherType.SelectedIndex = 0;
        txtVoucherNumber.Text = string.Empty;
        txtTransactionDate.Text = string.Empty;
        lblMessage.Text = "Transaction Updated";

    }

    protected void ddlVoucherType_SelectedIndexChanged(object sender, EventArgs e)
    {
        lblMessage.Text = string.Empty;
    }

    protected void grdTransaction_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        int transactionMasterId = Convert.ToInt32(ViewState["TransactionMasterId"]);
        SqlCommand cmd = new SqlCommand();
        cmd.CommandText = "DeleteTransaction";
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Connection = con;
        cmd.Parameters.Add("@TransactionMasterId", SqlDbType.Int).Value = transactionMasterId;
        cmd.ExecuteNonQuery();
        cmd.Dispose();
        grdTransaction.DataBind();
    }



    protected void btnLogout_Click(object sender, EventArgs e)
    {
        //FormsAuthentication.SignOut();
        // FormsAuthentication.RedirectToLoginPage();
        Session.Remove("Login");
        Response.Redirect("Login.aspx");
     
    }
}