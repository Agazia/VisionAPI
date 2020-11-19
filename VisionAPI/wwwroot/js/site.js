// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Upload Profile Picture (Portrait)

$('#uploadpic').click(function () {
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
                    $('#res').load("/Home/LoadResult", message);
                    $('#modal-content').html('<p class="font-weight-bold text-success"><i class="fas fa-check-circle"></i>Picture uploaded successfully</p><button id="uploaded" type="button" class="btn btn-sm btn-primary" style="width: 8rem;" data-dismiss="modal">Ok</button>');
                    $('#inputpic').val("");
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

$('#deletepic').click(function () {
    $.ajax({
        url: "/Home/DeletePicture",
        type: "POST",
        contentType: false,
        processData: false,
        success: function (message) {
            $('#result').html('<h4 class="mt-3 text-center">Deleted</h4>');
        },
        error: function () {
            alert("Error deleting the picture!");
        }
    })
});