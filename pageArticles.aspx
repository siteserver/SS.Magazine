<%@ Page Language="C#" Inherits="SS.Magazine.Pages.PageArticles" %>
  <%@ Register TagPrefix="ctrl" Namespace="SS.Magazine.Controls" Assembly="SS.Magazine" %>
    <!DOCTYPE html>
    <html>

    <head>
      <meta charset="utf-8">
      <link href="assets/plugin-utils/css/bootstrap.min.css" rel="stylesheet" type="text/css" />
      <link href="assets/plugin-utils/css/plugin-utils.css" rel="stylesheet" type="text/css" />
      <link href="assets/plugin-utils/css/font-awesome.min.css" rel="stylesheet" type="text/css" />
      <script src="assets/js/jquery-1.9.1.min.js"></script>
      <script src="assets/js/sweetalert-2.0.3.min.js"></script>
      <script src="assets/plugin-utils/js/bootstrap.min.js"></script>
    </head>

    <body>
      <form runat="server">

        <!-- container start -->
        <div class="container">
          <div class="m-b-25"></div>

          <div class="row">
            <div class="col-sm-12">
              <div class="card-box">
                <h4 class="text-dark  header-title m-t-0">文章管理</h4>
                <p class="text-muted m-b-25 font-13"></p>
                <asp:Literal id="LtlMessage" runat="server" />

                <table class="table table-bordered table-hover m-0">
                  <thead>
                    <tr class="info thead">
                      <th>标题</th>
                      <th style="width:160px;"></th>
                      <th class="text-center" style="width:100px;">操作</th>
                      <th class="text-center" width="20">
                        <input onclick="var checked = this.checked;$(':checkbox').each(function(){$(this)[0].checked = checked;checked ? $(this).parents('tr').addClass('success') : $(this).parents('tr').removeClass('success')});"
                          type="checkbox" />
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    <asp:Repeater ID="RptArticles" runat="server">
                      <itemtemplate>
                        <tr onClick="$(this).toggleClass('success');$(this).find(':checkbox')[0].checked = $(this).hasClass('success');">
                          <td>
                            <asp:Literal ID="ltlTitle" runat="server"></asp:Literal>
                          </td>
                          <td class="text-center">
                            <asp:Literal ID="ltlIsFree" runat="server"></asp:Literal>
                          </td>
                          <td class="text-center">
                            <asp:Literal ID="ltlActions" runat="server"></asp:Literal>
                          </td>
                          <td class="text-center">
                            <input type="checkbox" name="idCollection" value='<%#DataBinder.Eval(Container.DataItem, "Id")%>' />
                          </td>
                        </tr>
                      </itemtemplate>
                    </asp:Repeater>
                  </tbody>
                </table>

                <div class="m-b-25"></div>

                <ctrl:sqlPager id="SpArticles" runat="server" class="table table-pager" />

                <asp:Button class="btn btn-success" id="BtnAdd" Text="添 加" runat="server" />
                <asp:Button class="btn" id="BtnTaxis" Text="排 序" runat="server" />
                <asp:Button class="btn" id="BtnDelete" Text="删 除" runat="server" />

              </div>
            </div>
          </div>
        </div>
        <!-- container end -->

        <!-- modalAdd start -->
        <asp:PlaceHolder id="PhModalAdd" visible="false" runat="server">
          <div id="modalAdd" class="modal fade">
            <div class="modal-dialog" style="width:80%;">
              <div class="modal-content">
                <div class="modal-header">
                  <button type="button" class="close" onClick="$('#modalAdd').modal('hide');return false;" aria-hidden="true">×</button>
                  <h4 class="modal-title" id="modalLabel">
                    <asp:Literal id="LtlModalAddTitle" runat="server"></asp:Literal>
                  </h4>
                </div>
                <div class="modal-body">
                  <asp:Literal id="LtlModalAddMessage" runat="server"></asp:Literal>

                  <div class="form-horizontal">

                    <div class="form-group">
                      <label class="col-sm-2 control-label">文章标题</label>
                      <div class="col-sm-8">
                        <asp:TextBox id="TbTitle" class="form-control" runat="server"></asp:TextBox>
                      </div>
                      <div class="col-sm-2">
                          <asp:RequiredFieldValidator ControlToValidate="TbTitle" errorMessage=" *" foreColor="red" display="Dynamic" runat="server"
                          />
                      </div>
                    </div>

                    <div class="form-group">
                      <label class="col-sm-2 control-label">是否免费</label>
                      <div class="col-sm-4">
                        <asp:DropDownList ID="DdlIsFree" class="form-control" runat="server">
                          <asp:ListItem Text="免费阅读" Value="True" Selected="True" />
                          <asp:ListItem Text="付费阅读" Value="False" />
                        </asp:DropDownList>
                      </div>
                      <div class="col-sm-6">
                        <span class="help-block"></span>
                      </div>
                    </div>

                    <div class="form-group">
                      <div class="col-sm-12">
                        <ctrl:UEditor id="TbContent" runat="server"></ctrl:UEditor>
                      </div>
                    </div>

                  </div>
                </div>
                <div class="modal-footer">
                  <asp:Button class="btn btn-primary" onclick="BtnAdd_OnClick" runat="server" Text="保 存"></asp:Button>
                  <button type="button" class="btn btn-default" onClick="$('#modalAdd').modal('hide');return false;">取 消</button>
                </div>
              </div>
            </div>
          </div>
          <script>
            $(document).ready(function () {
              $('#modalAdd').modal();
            });
          </script>
        </asp:PlaceHolder>
        <!-- modalAdd end -->

        <!-- modalTaxis start -->
        <asp:PlaceHolder id="PhModalTaxis" visible="false" runat="server">
          <div id="modalTaxis" class="modal fade">
            <div class="modal-dialog" style="width:60%;">
              <div class="modal-content">
                <div class="modal-header">
                  <button type="button" class="close" onClick="$('#modalTaxis').modal('hide');return false;" aria-hidden="true">×</button>
                  <h4 class="modal-title" id="modalLabel">
                    文章排序
                  </h4>
                </div>
                <div class="modal-body">
                  <div class="form-horizontal">

                    <div class="form-group">
                      <label class="col-sm-2 control-label">排序方向</label>
                      <div class="col-sm-4">
                        <asp:DropDownList ID="DdlIsTaxisUp" class="form-control" runat="server">
                          <asp:ListItem Text="上升" Value="True" Selected="True" />
                          <asp:ListItem Text="下降" Value="False" />
                        </asp:DropDownList>
                      </div>
                      <div class="col-sm-6">
                        <span class="help-block"></span>
                      </div>
                    </div>
                    <div class="form-group">
                      <label class="col-sm-2 control-label">移动数目</label>
                      <div class="col-sm-8">
                        <asp:TextBox id="TbTaxisCount" class="form-control" runat="server" value="1"></asp:TextBox>
                      </div>
                      <div class="col-sm-2">
                          <asp:RequiredFieldValidator ControlToValidate="TbTaxisCount" errorMessage=" *" foreColor="red" display="Dynamic" runat="server"
                          />
                          <asp:RegularExpressionValidator ControlToValidate="TbTaxisCount" runat="server" ValidationExpression="^[0-9]\d*(\.\d+)?$"
                            ErrorMessage=" *" foreColor="red"></asp:RegularExpressionValidator>
                      </div>
                    </div>

                  </div>
                </div>
                <div class="modal-footer">
                  <asp:Button class="btn btn-primary" onclick="BtnTaxis_OnClick" runat="server" Text="提 交"></asp:Button>
                  <button type="button" class="btn btn-default" onClick="$('#modalTaxis').modal('hide');return false;">取 消</button>
                </div>
              </div>
            </div>
          </div>
          <script>
            $(document).ready(function () {
              $('#modalTaxis').modal();
            });
          </script>
        </asp:PlaceHolder>
        <!-- modalTaxis end -->
      </form>
    </body>

    </html>