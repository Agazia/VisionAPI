// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Upload Profile Picture (Portrait)

$(document).on('click', '.uploadpic', function () {
    $('#loadingModal').modal('show')
    $('#modal-content').html('<div class="spinner-border" role="status">\n</div >\n<h4>Das Bild wird analysiert</h4>\n<p>Bitte um etwas Geduld!</p>');
    if (window.FormData !== undefined) {
        var picUpload = $('#inputpic').get(0);
        var pic = picUpload.files;

        if (pic.length == 0) {
            $('#modal-content').html('<p class="font-weight-bold text-danger"><i class="fas fa-info"></i> No Picture selected!</p><button type="button" class="btn btn-sm btn-primary" style="width: 8rem;" data-dismiss="modal">Ok</button>');
        } else {
            var data = new FormData();

            for (var i = 0; i < pic.length; i++) {
                data.append(pic[i].name, pic[i]);
            }

            $.ajax({
                url: "/Home/UploadLocal",
                type: "POST",
                contentType: false,
                processData: false,
                data: data,
                success: function (message) {
                    if (message.url.includes('.png')) {
                        $('#res').load("/Home/LoadResult", message);
                        $('#modal-content').html('<p class="font-weight-bold text-success"><i class="fas fa-check-circle"></i> Das Bild wurde analysiert!</p><button id="uploaded" type="button" class="btn btn-sm btn-primary" style="width: 8rem;" data-dismiss="modal">Ok</button>');
                        $('#inputpic').val("");
                    }
                    else {
                        $('#modal-content').html('<p class="font-weight-bold text-danger"><i class="fas fa-info"></i> ' + message.url + '</p><button type="button" class="btn btn-sm btn-primary" style="width: 8rem;" data-dismiss="modal">Ok</button>');
                    }
                },
                error: function () {
                    $('#modal-content').html('<p class="font-weight-bold text-danger"><i class="fas fa-info"></i> There was error uploaing files! Please try again</p><button type="button" class="btn btn-sm btn-primary" style="width: 8rem;" data-dismiss="modal">Ok</button>');
                }
            }).done();
        }
    } else {
        $('#modal-content').html('<p class="font-weight-bold text-danger"><i class="fas fa-info"></i> An error occured while processing the file!</p><button type="button" class="btn btn-sm btn-primary" style="width: 8rem;" data-dismiss="modal">Ok</button>');
    }
});

$(document).on('click', '.deletepic', function () {

    $('#loadingModal').modal('show')
    $('#modal-content').html('<div class="spinner-border" role="status">\n</div >\n<h4>Das Bild wird gelöscht</h4>\n<p>Bitte um etwas Geduld!</p>');

    var imgId = $('.imageai').attr('id');

    $.post('Home/DeletePicture',
        {
            id: imgId
        },
        function (data, status) {
            if (status == 'success') {
                if (data.includes('Nichts')) {
                    $('#modal-content').html('<p class="font-weight-bold"><i class="fas fa-info"></i> ' + data + '</p><button type="button" class="btn btn-sm btn-primary" style="width: 8rem;" data-dismiss="modal">Ok</button>');
                } else {
                    $('#result').remove();
                    $('#modal-content').html('<p class="font-weight-bold text-danger"><i class="fas fa-info"></i> ' + data + '</p><button type="button" class="btn btn-sm btn-primary" style="width: 8rem;" data-dismiss="modal">Ok</button>');
                }
            } else {
                $('#modal-content').html('<p class="font-weight-bold text-danger"><i class="fas fa-info"></i> An error occured while deleting the file!</p><button type="button" class="btn btn-sm btn-primary" style="width: 8rem;" data-dismiss="modal">Ok</button>');
            }
        });
});


