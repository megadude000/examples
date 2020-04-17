(function () {
    'use strict';

    angular
        .module('Company.gazoo.examination')
        .controller('audioRecordingController', audioRecordingController)
        .config(['recorderServiceProvider', function (recorderServiceProvider) {
            recorderServiceProvider
                .forceSwf(window.location.search.indexOf('forceFlash') > -1)
                .setSwfUrl('/lib/recorder.swf');
        }])
        .directive('ngWavesurfer', function () {
            return {
                restrict: 'E',

                link: function ($scope, $element, $attrs) {
                    $element.css('display', 'block');

                    var options = angular.extend({ container: $element[0] }, $attrs);
                    var wavesurfer = WaveSurfer.create(options);

                    if ($attrs.url) {
                        wavesurfer.loadBlob($attrs.url, $attrs.data || null);
                    }

                    $scope.$emit('wavesurferInit', wavesurfer);
                }
            };
        })
        .factory('Data', function () {
            return { Aduio: new Blob() };
        });

    angular
        .module('Company.gazoo.examination')
        .component('audioFilesComponent.audioRecording', {
            controller: 'audioRecordingController',
            templateUrl: '/templates/examination/audioRecording.html'
        });

    audioRecordingController.$inject = ['$scope', '$ngBootbox', 'recorderService', 'managingFilesShareDataService', 'alertService', '$translate', 'examinationHttpService', 'promiseCommonService'];

    function audioRecordingController($scope, $ngBootbox, recorderService, managingFilesShareDataService, alertService, $translate, examinationHttpService, promiseCommonService) {
        $scope.fileInfo = {};
        $scope.recordedInput = null;
        $scope.paused = true;
        $scope.isRecording = false;
        $scope.timerRunning = true;
        $scope.canSave = false;
        $scope.validName = false;

        $scope.saveAudioResult = {
            Ok: 0,
            Error: 1
        };

        $scope.startRecorderingTemp = function () {
            navigator.getUserMedia = navigator.getUserMedia ||
                navigator.webkitGetUserMedia ||
                navigator.mediaDevices.getUserMedia;

            if (navigator.getUserMedia) {
                $scope.wavesurfer.stop();
                console.log('getUserMedia supported.');
                $scope.startTimer();
                navigator.getUserMedia(
                    {
                        audio: true
                    },

                    function (stream) {
                        var options = {
                            recorderType: StereoAudioRecorder,
                            mimeType: 'audio/wav',
                            desiredSampRate: 8 * 1000,
                            numberOfAudioChannels: 1,
                            disableLogs: true
                        };
                        $scope.recordering = RecordRTC(stream, options);
                        $scope.recordering.startRecording();
                        startTimer();
                    },

                    function (err) {
                        console.log('The following gUM error occured: ' + err);
                    }
                );
            } else {
                console.log('getUserMedia not supported on your browser!');
            }
        };

        $scope.stopRecorderingTemp = function () {
            $scope.recordering.stopRecording(function (audioVideoWebMURL) {
                $scope.recordedInput = this.getBlob();

                loadAudio();
                $scope.canSave = true;
            });
        };

        $scope.startTimer = function () {
            $scope.timerRunning = true;
        };

        $scope.stopTimer = function () {
            $scope.timerRunning = false;
        };

        $scope.changeToRecordStatus = function () {
            $scope.wavesurfer.stop();
            $scope.isRecording = true;
        };

        $scope.$on('wavesurferInit', function (e, wavesurfer) {
            $scope.wavesurfer = wavesurfer;

            $scope.wavesurfer.on('play', function () {
                $scope.paused = false;
            });

            $scope.wavesurfer.on('pause', function () {
                $scope.paused = true;
            });

            $scope.wavesurfer.on('finish', function () {
                $scope.paused = true;
                $scope.wavesurfer.seekTo(0);
                $scope.$apply();
            });
        });

        $scope.confirmSaveAudioMessage = function () {
            $ngBootbox.confirm($translate.instant("SAVE_AUDIO_QUESTION_FILES_CONFIRMATION"))
                .then(function () {
                    uploadFileAndInfo();
                });
        };

        function loadAudio() {
            $scope.isRecording = false;
            $scope.wavesurfer.loadBlob($scope.recordedInput);
            managingFilesShareDataService.setAudio($scope.recordedInput);
        }

        function startTimer() {
            $scope.isRecording = true;
            $scope.canSave = false;
            $scope.$apply();
        }

        function uploadFileAndInfo() {
            $scope.fileInfoModel = managingFilesShareDataService.getAudio();

            if (isFileValid($scope.fileInfoModel))
                uploadFileInfoToServer();
            else
                alertService.showError("UPLOADING_FILE_HAS_INCORRECT_TYPES");
        }

        $scope.validateAudioFileInfo = function () {
            checkIfFileExistOnServer($scope.audioFileInfo.fileName);
        };

        function checkIfFileExistOnServer(fileName) {
            $scope.checkIfFileExistOnServerPromise = promiseCommonService.createPromise(
                examinationHttpService.checkIfFileExistOnServer,
                fileName,
                "ERROR_DURING_CREATING_FILE",
                function (response) {
                    if (response.data) {
                        $scope.fileInfo['fileName'].$setValidity("validName", false);
                        alertService.showWarning("FILE_EXISTS_IN_DB");
                        return;
                    }

                    $scope.fileInfo['fileName'].$setValidity("validName", true);
                });
        }

        var isFileValid = function (file) {
            return !(file.size === 0 || file.type !== "audio/wav");
        };

        var uploadFileInfoToServer = function () {
            var formDataObject = getFormDataObjectForFileInfo();
            $scope.uploadAudioFilesPromise = promiseCommonService.createPromise(
                examinationHttpService.uploadAudioFilesPromise,
                formDataObject,
                "ERROR_DURING_CREATING_FILE",
                function (response) {
                    if (response.data !== $scope.saveAudioResult.Ok) {
                        alertService.showError("UPLOADING_FILE_ERROR_MESSAGE");
                        resetAudioFileInfoName();
                        return;
                    }
                    else if (response.data === $scope.saveAudioResult.Ok) {
                        alertService.showSuccess("UPLOADING_FILE_SUCCESS_MESSAGE");
                        resetAudioFileInfo();
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

        var resetAudioFileInfoName = function () {
            $scope.audioFileInfo.fileName = "";
        };

        var resetAudioFileInfo = function () {
            $scope.audioFileInfo = createAudioFileInfo();
            $scope.fileInfoModel = null;
            $scope.canSave = false;
            $scope.recordedInput = null;
            $scope.validName = false;
        };

        function createAudioFileInfo() {
            return {
                fileName: "",
                comment: ""
            };
        }
    }
})();