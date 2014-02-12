$(document).ready(function () {

  //run at startup
  if ($("#RuleActionId").val() == "3") {
      HideFieldsOnDelete();
      $('#ChoiceReplaceByValue').prop('checked', true);
  }
  else {
      if ($("#ReplaceColumn").val().length > 0) {
          HideFieldsOnReplaceColumnSelected();
          $('#ChoiceReplaceByColumn').prop('checked', true);
      }
      else {
          HideFieldsOnReplaceValueSelected();
          $('#ChoiceReplaceByValue').prop('checked', true);
      }
  }

  $('#ChoiceReplaceByValue').click(function (e) {
    HideFieldsOnReplaceValueSelected();
    e.stopPropagation();
  });

  $('#ChoiceReplaceByColumn').click(function (e) {
    HideFieldsOnReplaceColumnSelected();
    e.stopPropagation();
  });

  $('#RuleActionId').change(function () {
    switch ($(this).val()) {
      case "3": //delete selected
        HideFieldsOnDelete();
        break;
      default:
        ShowFieldsOnNoDelete();
        break;
    }
  });

  function HideFieldsOnReplaceColumnSelected() {
    ShowFields(["EditReplaceColumn"]);
    HideFields(["EditReplaceValue"]);
    $('#ReplaceValue').val("");
  }

  function HideFieldsOnReplaceValueSelected() {
    ShowFields(["EditReplaceValue"]);
    HideFields(["EditReplaceColumn"]);
    $('#ReplaceColumn').val("");
  }

  function HideFieldsOnDelete() {
    HideFields(["TableAction"]);
    $('#ReplaceColumn').val("");
    $('#ReplaceValue').val("");
  }

  function ShowFieldsOnNoDelete() {
    ShowFields(["TableAction"]);
    HideFieldsOnReplaceValueSelected();
  }

  function ShowFields(fieldsArray) {
    if (fieldsArray == null)
      return;

    $(fieldsArray).each(function (index, value) {
      $('#' + value).css("display", "block");
    });
  }

  function HideFields(fieldsArray) {
    if (fieldsArray == null)
      return;

    $(fieldsArray).each(function (index, value) {
      $('#' + value).css("display", "none");
    });
  }
});