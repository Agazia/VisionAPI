// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Upload Profile Picture (Portrait)


function analyze_imgAndSpeech() {
    $('#loadingModal').modal('show');
    $('#modal-content').html(returnLoadingMsg('Das Bild wird analysiert'));

    var picurl = $("#inputurl").val();
    var picUpload = $('#inputpic').get(0);
    var data = new FormData();

    if (picUpload.files.length !== 0) {
        var pic = picUpload.files;

        for (var i = 0; i < pic.length; i++) {
            data.append(pic[i].name, pic[i]);
        }

        data.append('picurl', picurl);

        $.when(show_result(data)).done(function () { delete_all() });

    } else if (picurl !== "") {
        data.append('picurl', picurl);
        show_result(data);
    } else {
        $('#modal-content').html(returnErrorMsg('No valid image/url were specified'));
    }
};


function show_result(data) {

    $.ajax({
        url: "/Home/AnalyzeImageAndSound",
        type: "POST",
        contentType: false,
        processData: false,
        data: data,
        success: function (message) {
            if (message.message == "Success") {
                $('#resultsection').load("/Home/LoadResult", message);
                $('#modal-content').html(returnSuccessMsg('Das Bild wurde analysiert!'));
                $('#inputpic').val("");
                $("#inputurl").val("");
            }
            else {
                $('#modal-content').html(returnErrorMsg(message.message));
                $('#inputpic').val("");
                $("#inputurl").val("");
            }
        },
        error: function () {
            $('#modal-content').html(returnErrorMsg('An error occured while analying the image!'));
        }
    }).done();
}


function delete_img(url) {

    $('#loadingModal').modal('show');
    $('#modal-content').html(returnLoadingMsg('Das Bild wird gelöscht'));

    $.post('Home/DeletePicture',
        {
            id: url
        },
        function (data, status) {
            if (status == 'success') {
                if (!data) {
                    $('#modal-content').html(returnInfoMessage('Nichts zu löschen!'));
                } else {
                    $('#result').remove();
                    $('#modal-content').html(returnSuccessMsg('Das Bild wurde gelöscht!'));
                }
            } else {
                $('#modal-content').html(returnErrorMsg('An error occured while deleting the file!'));
            }
        });
};

function delete_all() {
    $.post('Home/DeleteAll', function (status) {

    });
};


function analyze_speech() {

    $('#loadingModal').modal('show');
    $('#modal-content').html(returnLoadingMsg('Der Text wird umgewandelt'));

    var msg = $('textarea#message').val();

    $.post('Home/UploadSpeech', {
        id: msg
    }, function (data, status) {
            if (status == 'success') {
                if (!data) {
                    $('#modal-content').html(returnErrorMsg('An error occured while analyzing the text!'));

                } else {
                    $('#playspeech').load("/Home/PlaySpeech", data);
                    $('#loadingModal').modal('hide');
                }
            } else {
                $('#modal-content').html(returnErrorMsg('An error occured while analyzing the text!'));
            }
    });
};



function returnErrorMsg(message) {
    return '<p class="font-weight-bold text-danger"><i class="fas fa-info"></i> ' + message + ' </p><button type="button" class="btn btn-sm btn-primary" style="width: 8rem;" data-dismiss="modal">Ok</button>';
}

function returnSuccessMsg(message) {
    return '<p class="font-weight-bold text-success"><i class="fas fa-check-circle"></i> ' + message + '</p><button type="button" class="btn btn-sm btn-primary" style="width: 8rem;" data-dismiss="modal">Ok</button>';
}

function returnLoadingMsg(message) {
    return '<div class="spinner-border" role="status">\n</div >\n<h4>' + message + ' </h4>\n<p>Bitte um etwas Geduld!</p>';
}

function returnInfoMessage(message) {
    return '<p class="font-weight-bold"><i class="fas fa-info"></i> ' + message + '</p><button type="button" class="btn btn-sm btn-primary" style="width: 8rem;" data-dismiss="modal">Ok</button>';
}


