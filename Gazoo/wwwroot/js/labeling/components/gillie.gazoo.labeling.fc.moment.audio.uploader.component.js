(function () {
    'use strict';

    angular
        .module('Company.gazoo.labeling')
        .controller('fcMomentsUploaderController', fcMomentsUploaderController);

    angular
        .module('Company.gazoo.labeling')
        .component('masterActionsModule.audioUploadComponent.fcMomentUpload', {
            controller: 'fcMomentsUploaderController',
            templateUrl: '/templates/labeling/fcMomentsAudioUploader.html'
        });

    fcMomentsUploaderController.$inject = ['$q', '$scope', 'fcMomentHttpService', 'promiseCommonService', 'transacriptionHttpService'];

    function fcMomentsUploaderController($q, $scope, fcMomentHttpService, promiseCommonService, transacriptionHttpService) {
        var csvColumnSeparator = ';';
        var uploadNumber = 0;
        var importNumber = null;
        var labelingType = LabelingType;

        $scope.totalUploadNumber = 0;
        $scope.comment = null;
        $scope.fcAudios = [];
        $scope.uploadingProgress = 0;
        $scope.priorityLevels = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        $scope.priority = $scope.priorityLevels[0];

        $scope.initialize = function () {
            setHandleOnChangeFileInput();
        };

        $scope.saveAudioResult = {
            Ok: 0,
            Error: 1
        };


        $scope.uploadAll = function () {
            $scope.totalUploadNumber = $scope.fcAudios.length;
            getNewImportNumber();

            $q.when($scope.getImportNumberPromise).then(function () {
                uploadAudios();
            });
        };

        function getNewImportNumber() {
            var request = {
                comment: $scope.comment,
                priority: $scope.priority,
                type: labelingType.FullConversationMoments
            };

            $scope.getImportNumberPromise = promiseCommonService.createPromise(
                transacriptionHttpService.getNewImportNumber,
                request,
                "ERROR",
                function (response) {
                    importNumber = response.data;
                });
        }

        function uploadAudios() {
            var formDataObject = {};
            var result = findAndParseAllMomentsForAudio($scope.fcAudios[0]);

            $q.all(result.promises).then(function () {
                formDataObject = getFormDataObjectForFileInfo(result.object);

                if (!formDataObject)
                    uploadNext();

                $scope.saveAudioPromise = promiseCommonService.createPromise(
                    fcMomentHttpService.saveAudioWithData,
                    formDataObject,
                    "ERROR_DURING_CREATING_FILE",
                    function () {
                        uploadNext();
                    },
                    function () {
                        console.log($scope.fcAudios[0] + " failed to upload!");
                        uploadNext();
                    });
            });
        }

        function uploadNext() {
            $scope.fcAudios.shift();
            uploadNumber++;
            if (!$scope.fcAudios.length) {
                toDefaults();
                return;
            }
            $scope.uploadingProgress = (uploadNumber / $scope.totalUploadNumber) * 100;
            uploadAudios();
        }

        function isSubfolderEquals(path, folderName) {
            return path.split('/')[1].indexOf(folderName) > -1;
        }

        function findAndParseAllMomentsForAudio(audio) {
            var audioInfo = {
                callId: audio
            };

            var dataForAudio = [];
            var outputForAudio = [];

            $scope.selectedFiles.forEach(function (entry) {
                if (entry.name.split('_')[0] === audioInfo.callId && isSubfolderEquals(entry.webkitRelativePath,"data")) {
                    dataForAudio.push(entry);
                    return;
                }
                if (entry.name.split('_')[0] === audioInfo.callId && isSubfolderEquals(entry.webkitRelativePath, "output")) {
                    outputForAudio.push(entry);
                    return;
                }
            });

            var object = {
                audioInfo: audioInfo,
                audioFile: null,
                momentData: null
            };

            var momentData = [];
            object.momentData = momentData;
            if (dataForAudio.length !== outputForAudio.length)
                return null;

            var promises = [];
            dataForAudio.forEach(function (entry) {
                var momentDataItem = {
                    audioMessageLog: null,
                    possibleAms: null,
                    sourceInputName: null
                };

                var outputFile = outputForAudio.find(obj => {
                    return obj.name === entry.name;
                });

                momentDataItem.sourceInputName = entry.name.split('.').slice(0, -1).join('.');
                var dataPromise = parseDataFile(entry).then(function(value) {
                    momentDataItem.audioMessageLog = value;
                });
                promises.push(dataPromise);
                var outputPromise = parseOutputFile(outputFile).then(function (value) {
                    momentDataItem.possibleAms = value;
                });
                promises.push(outputPromise);
                momentData.push(momentDataItem);
            });
            return {
                promises: promises,
                object: object
            };
        }

        function toDefaults() {
            $('#fullConversationMomentsFileInput').val('');
            $scope.fcAudios = [];
            $scope.uploadingProgress = 0;
            $scope.totalUploadNumber = 0;
            $scope.comment = null;
            $scope.priority = $scope.priorityLevels[0];
            importNumber = null;
            uploadNumber = 0;
        }

        function getFormDataObjectForFileInfo(entry) {
            var formDataObjectForFileInfo = new FormData();
            entry.audioInfo.importNumber = importNumber;
            formDataObjectForFileInfo.append('audioFileInfo', JSON.stringify(entry.audioInfo));
            formDataObjectForFileInfo.append('momentData', JSON.stringify(entry.momentData));
            formDataObjectForFileInfo.append('file', $scope.selectedFiles.find(obj => {
                return obj.webkitRelativePath.indexOf("audio") > -1 && obj.name.indexOf(entry.audioInfo.callId) > -1;
            }));

            return formDataObjectForFileInfo;
        };

        function setHandleOnChangeFileInput() {
            $('#fullConversationMomentsFileInput').on('change', function (e) {
                $scope.selectedFiles = Array.from(this.files);
                getAudios();
                $scope.$apply();
            });
        }

        function getAudios() {
            $scope.selectedFiles.forEach(function (entry) {
                if (entry.webkitRelativePath.indexOf("audio") > -1) {
                    $scope.fcAudios.push(entry.name.split('.').slice(0, -1).join('.'));
                    return;
                }
            });
        }

        function parseDataFile(file) {
            return new Promise((resolve, reject) => {
                var fileReader = new FileReader();
                var amHistory = [];
                fileReader.readAsText(file);
                fileReader.onerror = reject;
                fileReader.onload = function () {
                    var splitted = fileReader.result.split(/\r\n|\n/);
                    splitted.shift();

                    splitted.forEach(function (entry) {
                        var splittedRow = entry.split(csvColumnSeparator);
                        if (splittedRow.length > 1) {
                            var amLog = {
                                audioName: splittedRow[0],
                                completeness: splittedRow[1]
                            };
                            amHistory.push(amLog);
                        }
                    });
                    resolve(amHistory);
                };
            });
        }

        function parseOutputFile(file) {
            return new Promise((resolve, reject) => {
                var fileReader = new FileReader();
                var ams = [];
                fileReader.readAsText(file);
                fileReader.onerror = reject;
                fileReader.onload = function () {
                    var splitted = fileReader.result.split(/\r\n|\n/);
                    splitted.shift();

                    splitted.forEach(function (entry) {
                        var splittedRow = entry.split(csvColumnSeparator);
                        if (splittedRow.length > 1) {
                            var am = {
                                audioMessage: splittedRow[0],
                                confidence: splittedRow[1]
                            };
                            ams.push(am);
                        }
                    });
                    resolve(JSON.stringify(ams));
                };
            });
        }
    }
})();