(function () {
    'use strict';

    angular
        .module('Company.gazoo.examination')
        .controller('audioUploaderController', audioUploaderController);

    angular
        .module('Company.gazoo.examination')
        .component('audioFilesComponent.audioUploader', {
            controller: 'audioUploaderController',
            templateUrl: '/templates/examination/audioUploader.html'
        });

    audioUploaderController.$inject = ['$scope', '$translate', 'alertService', '$ngBootbox', 'FileUploader', 'examinationHttpService', 'promiseCommonService'];

    function audioUploaderController($scope, $translate, alertService, $ngBootbox, FileUploader, examinationHttpService, promiseCommonService) {
        $scope.fileUpload = {};
        $scope.fileInfoModel = null;
        $scope.isFileExistsOnServer = false;
        $scope.blockWhileCheking = false;
        $scope.notProperFormat = false;
        var uploader = $scope.uploader = new FileUploader({
            url: 'api/ExaminationAudio/SaveExaminationAudio'
        });

        $scope.audioFileInfo = {
            fileName: "",
            comment: ""
        };

        // a sync filter
        uploader.filters.push({
            name: 'syncFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                uploader.clearQueue();
                var type = item.type.slice(item.type.lastIndexOf('/') + 1);
                if (type !== 'wav') {
                    $scope.notProperFormat = true;
                    return;
                }
                else {
                    $scope.notProperFormat = false;
                    return this.queue.length < 10;
                }
            }
        });

        $scope.confirmUploadAudioMessage = function () {
            $ngBootbox.confirm($translate.instant("UPLOAD_AUDIO_FILES_CONFIRMATION"))
                .then(function () {
                    uploadFileAndInfo();
                });
        };

        function uploadFileAndInfo() {
            $scope.fileInfoModel = uploader.queue[0]._file;
            uploadFileInfoToServer();
        }

        $scope.saveAudioResult = {
            Ok: 0,
            Error: 1
        };

        function toDefaults() {
            $scope.audioFileInfo = {
                fileName: "",
                comment: ""
            };

            $('#fileInput').val('');
        }

        var uploadFileInfoToServer = function () {
            var formDataObject = getFormDataObjectForFileInfo();
            $scope.uploadAudioFilesPromise = promiseCommonService.createPromise(
                examinationHttpService.uploadAudioFilesPromise,
                formDataObject,
                "ERROR_DURING_CREATING_FILE",
                function (response) {
                    if (response.data !== $scope.saveAudioResult.Ok) {
                        alertService.showError("UPLOADING_FILE_ERROR_MESSAGE");
                        toDefaults();
                        return;
                    }
                    else if (response.data === $scope.saveAudioResult.Ok) {
                        alertService.showSuccess("UPLOADING_FILE_SUCCESS_MESSAGE");
                        toDefaults();
                        return;
                    }
                });
        };

        var getFormDataObjectForFileInfo = function () {
            var formDataObjectForFileInfo = new FormData();
            formDataObjectForFileInfo.append('audioFileInfo', JSON.stringify($scope.audioFileInfo));
            formDataObjectForFileInfo.append('file', $scope.fileInfoModel);

            return formDataObjectForFileInfo;
        };

        $scope.checkIfFileExistOnServer = function (fileName) {
            $scope.blockWhileCheking = true;
            $scope.checkIfFileExistOnServerPromise = promiseCommonService.createPromise(
                examinationHttpService.checkIfFileExistOnServer,
                fileName,
                "ERROR_DURING_UPDATING_FILES",
                function (response) {
                    if (response.data) {
                        $scope.fileUpload['fileName'].$setValidity("validName", false);
                        alertService.showWarning("FILE_EXISTS_IN_DB");
                        $scope.blockWhileCheking = false;
                        return;
                    }

                    $scope.fileUpload['fileName'].$setValidity("validName", true);
                    $scope.blockWhileCheking = false;
                });
        };
    }
})();