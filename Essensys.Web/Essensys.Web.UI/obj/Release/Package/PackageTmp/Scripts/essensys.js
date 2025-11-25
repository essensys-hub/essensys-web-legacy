/* Essensys.js
* 
* 20121218
*/
var cptalarme = 500;

$(document).ready(function () {

   $.each($(".esys-zone input[checked=checked]"), function (i, o) {
        $(o).attr('data-initial', 'true');
    });

    // Test attente box
    if ($('#esys-valid').attr('data-waitbox') == 'true') {
        $.colorbox({
            html: "<div class='esys-wait'>Synchronisation avec Essensys<br/>Veuillez patienter</div>",
            overlayClose: false,
            escKey: false
        });
        $('input').attr('disabled', 'disabled');
        $.post("/Home/WaitBox", function (data) {
            if (data.success) {
                allowform();
                $.colorbox.close();
                refreshzones(data.state);
            }
            else {
                $('.esys-wait').html(data.message + "<br/><a href='#'>Réessayer</a>");
                $('.esys-wait a').bind('click', function () {
                    $.colorbox.close();
                    window.location = "/";
                });
                $('.esys-wait').css('background', 'none');
            }
        });
    }

    if ($('form').attr('action') == '/Account/UpdateMyInfos'){
        $('form').attr('autocomplete', 'off');
    }

    // Téléphone
    $.each($('.esys-phonelist'), function(i1, o1){
        $.getJSON('/Phone/ListPhone', function(data){
            if (data.success){
                $.each(data.phones, function (i, o){
                    $('.esys-phonelist tbody').append("<tr data-id='" + o.Id + "'><td><img src='/Content/images/action_delete.gif' style='cursor:pointer' title='Cliquez ici pour supprimer la ligne' /></td><td><input type='text' class='phone-name' value='" + o.Nom + "' title='Saisissez ici pour le nom de la personne'/></td><td><input type='text' value='" + o.Phone + "' class='phone-value' title='Saisissez ici le numéro de téléphone'/></td></tr>");
                });
                registerphoneactions();
            }
        });
    });

    $(".esys-zone input[type=radio]").bind("click", function () {
        if ($(this).attr('data-initial') == 'true') {
            $(this).parent().parent().parent().find('span').hide();
            $(this).parent().parent().removeClass('tovalidate');
        }
        else {
            $(this).parent().parent().parent().find('span').show();
            $(this).parent().parent().addClass('tovalidate');
        }
    });
    $(".esys-zone input[type=checkbox]").bind("click", function () {
        if ($("#volet").attr('checked') == 'checked' || $("#store").attr('checked') == 'checked') {
            $(this).parent().parent().parent().find('span').show();
            $(this).parent().parent().addClass('tovalidate');
        }
        else {
            $(this).parent().parent().parent().find('span').hide();
            $(this).parent().parent().removeClass('tovalidate');
        }
    });
    $('#codealarme').bind("change", function () {
        if ($(this).val() != "") {
            $("#question").val("");
            allowalarme();
        } else {
            disallowalarme(true);
        }
    });
    $('#question').bind("change", function () {
        $.post("/Account/TestResponse", { res: $(this).val() }, function (data) {
            if (data.success) {
                if (data.responseisok)
                    allowalarme();
                else
                    disallowalarme();
            }
            else
                disallowalarme(true);
        });
    });
    $('#esys-undo').bind('click', function () {
        $("#question").val("");
        $("#codealarme").val("");
        $("input[data-initial=true]").click();
        disallowalarme(false);
    });

    $('#esys-close').bind('click', function () {
        $.colorbox({ overlayClose: false, html: "<div class='esMessage'>Voulez-vous réellement clôturer le compte ?</div><input type='button' value='Oui' id='esys-yes' /><input type='button' value='Non' id='esys-no' />" });
        $('#esys-no').bind('click', function () {
            $.colorbox.close()
        });
        $('#esys-yes').bind('click', function () {
            $.post('/Account/CloseAccount', function (data) {
                if (data.success) {
                    window.location = '/Account/CloseMessage';
                }
            });
        });
    });
    $('#esys-undo').bind('click', function () {
        window.location = "/Account/Logout";
    });
    $('#esys-valid').bind('click', function () {
        $.colorbox({
            html: "<div class='esys-wait'>Synchronisation avec Essensys<br/>Veuillez patienter</div>",
            overlayClose: false,
            escKey: false
        });
        $('input').attr('disabled', 'disabled');

        // Encodage du formulaire
        var newar = ($('#arrosagecontainer span').css('display') != 'none');
        var newal = ($('#alarmecontainer span').css('display') != 'none');
        var newcf = ($('#chauffagezjcontainer span').css('display') != 'none');
        var newcfzn = ($('#chauffagezncontainer span').css('display') != 'none');
        var newcfsdb1 = ($('#chauffagesdb1container span').css('display') != 'none');
        var newcfsdb2 = ($('#chauffagesdb2container span').css('display') != 'none');
        var newcm = ($('#cumuluscontainer span').css('display') != 'none');

        $.post("/Home/DoActions", {
            newar: newar,
            ar: $('input:radio[name=arrosage]:checked').val(),
            newal: newal,
            al: $('input:radio[name=alarme]:checked').val(),
            alresp: $("#question").val(),
            codealarme: $("#codealarme").val(),
            newcf: newcf,
            cfzj: $('input:radio[name=chauffagezj]:checked').val(),
            newcfzn: newcfzn,
            cfzn: $('input:radio[name=chauffagezn]:checked').val(),
            newcfsdb1: newcfsdb1,
            cfsdb1: $('input:radio[name=chauffagesdb1]:checked').val(),
            newcfsdb2: newcfsdb2,
            cfsdb2: $('input:radio[name=chauffagesdb2]:checked').val(),
            newcm: newcm,
            cfcm: $('input:radio[name=cumulus]:checked').val(),
            cfvol: $('input:checkbox[name=volet]:checked').val(),
            cfsto: $('input:checkbox[name=store]:checked').val(),
        }, function (data) {
            if (data.success) {
                allowform();
                $('.esys-zone').find('span').hide();
                $('.esys-validinfo').removeClass('tovalidate');
                $("input[data-initial=true]").removeAttr("data-initial");
                $("input[checked=checked]").attr("data-initial", "true");
                $('#volet').removeAttr('checked');
                $('#store').removeAttr('checked');
                $.colorbox.close();
                refreshzones(data.state);
            }
            else {
                $('.esys-wait').html(data.message + "<br/><a href='#'>Fermer</a>");
                $('.esys-wait a').bind('click', function () {
                    $.colorbox.close();
                });
                $('.esys-wait').css('background', 'none');
            }
        });
    });
});

function allowalarme() {
    $('.alarmeoption').removeClass('disabled');
    $('.alarmeoption input').removeAttr('disabled');
};
function disallowalarme(withcpt) {
    $('.alarmeoption').addClass('disabled');
    $('.alarmeoption input').attr('disabled', 'disabled');
    if (withcpt) {
        cptalarme = cptalarme * 2;
        disallowform();
        setTimeout(function () { allowform(); }, cptalarme);
    }
};
function disallowform() {
    $('body').css('opacity', '0.2');
    $('input').attr('disabled', 'disabled');
};
function allowform() {
    $('body').css('opacity', '1');
    $('input').removeAttr('disabled');
    disallowalarme(false);
}

function registerphoneactions(){
    $('.esys-phonelist input').bind('click', function(){
        if ($(this).val() == 'Entrez un nom' || $(this).val() == 'Entrez un numéro'){
            $(this).val('');
        }
    });
    $('.esys-phonelist input').bind('change', function(){
        if ($(this).val() != ''){
            savephone($(this));
        }
    });
    $('.esys-phonelist img').bind('click', function(){
        var id = $(this).parent().parent().attr('data-id');
        var l = $(this).parent().parent();
        $.post('/Phone/DeletePhone', { id: id}, function(data){
            if (data.success){
                l.find('.phone-value').val('Entrez un numéro');
                l.find('.phone-name').val('Entrez un nom');
            }
        });
    });
};

function savephone(obj){
    var id = obj.parent().parent().attr('data-id');
    var name = obj.parent().parent().find('.phone-name').val();
    if (name == "Entrez un nom")
        name = "";
    var phone = obj.parent().parent().find('.phone-value').val();
    if (phone == "Entrez un numéro")
        phone = "";
    $.post('/Phone/AddOrUpdatePhone', { id: id, phone: phone, nom: name}, function(data){
        if (data.success){
            obj.parent().parent().find('.phone-value').val(data.phone.Phone);

            obj.parent().parent().attr('data-id', data.phone.Id);
        }
    });
};

function refreshzones(data) {
    $.each(data, function (i, o) {
        console.log(o.Index.IndexKey);
        if (o.Index.IndexKey == "346") {
            $("#chauffagezj[value='" + o.Value + "']").attr('checked', 'checked');
            $("#chauffagezj[value='" + o.Value + "']").attr('data-initial', 'true');
        }
        if (o.Index.IndexKey == "347") {
            $("#chauffagezn[value='" + o.Value + "']").attr('checked', 'checked');
            $("#chauffagezn[value='" + o.Value + "']").attr('data-initial', 'true');
        }
        if (o.Index.IndexKey == "348") {
            $("#chauffagesdb1[value='" + o.Value + "']").attr('checked', 'checked');
            $("#chauffagesdb1[value='" + o.Value + "']").attr('data-initial', 'true');
        }
        if (o.Index.IndexKey == "349") {
            $("#chauffagesdb2[value='" + o.Value + "']").attr('checked', 'checked');
            $("#chauffagesdb2[value='" + o.Value + "']").attr('data-initial', 'true');
        }
        if (o.Index.IndexKey == "360") {
            $("#arrosage[value='" + o.Value + "']").attr('checked', 'checked');
            $("#arrosage[value='" + o.Value + "']").attr('data-initial', 'true');
        }
        if (o.Index.IndexKey == "350") {
            console.log(o.Value);
            console.log($("#cumulus[value='" + o.Value + "']").length);
            $("#cumulus[value='" + o.Value + "']").attr('checked', 'checked');
            $("#cumulus[value='" + o.Value + "']").attr('data-initial', 'true');
        }
    });
};