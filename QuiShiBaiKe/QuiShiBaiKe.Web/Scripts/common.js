function isEmpty(id,desc)
{
    var value = $("#" + id).val();
    if (value.length<=0) {
        alert(desc + "不能为空！");
        $("#" + id).focus();
        return false;
    }
    return true;
}