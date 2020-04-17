(function () {
    'use strict';

    angular
        .module('Company.gazoo.examination')
        .controller('audioFileListController', audioFileListController);

    angular
        .module('Company.gazoo.examination')
        .component('audioFilesComponent.audioFileList', {
            controller: 'audioFileListController',
            templateUrl: '/templates/examination/audioFileList.html'
        });

    audioFileListController.$inject = ['$scope', '$ngBootbox', 'examinationHttpService', 'orderByFilter', '$uibModal', '$translate', 'alertService', 'promiseCommonService'];

    function audioFileListController($scope, $ngBootbox, examinationHttpService, orderByFilter, $uibModal, $translate, alertService, promiseCommonService) {
        $scope.audioFiles = [];
        $scope.paginatedAudioFiles = [];
        $scope.minAmountOfLetters = 3;
        $scope.isCheckedAllFiles = false;
        $scope.fileNameFilter = "";
        $scope.isFileExistsOnServer = false;
        $scope.blockWhileCheking = false;
        $scope.pagination = {
            currentPage: 1,
            itemsPerPage: 25,
            totalItems: 0,
            maxVisiblePages: 7
        };

        $scope.initialize = function () {
            resetAudioFileInfo();
            $scope.fetchFiles();
        };

        $scope.setPage = function () {
            var skip = ($scope.pagination.currentPage - 1) * $scope.pagination.itemsPerPage;
            $scope.paginatedAudioFiles = $scope.audioFiles.slice(skip, skip + $scope.pagination.itemsPerPage);
        };

        $scope.fetchFiles = function () {
            if (isInputEmpty($scope.fileNameFilter)) {
                fetchAudioFiles($scope.fileNameFilter);
                return;
            }
            if ($scope.fileNameFilter.length !== 0 && !isQueryValid($scope.fileNameFilter)) {
                alertService.showWarning("MIN_AMOUNT_LETTERS");
                return;
            }
            if (hasInputNoizeCharacters($scope.fileNameFilter)) {
                alertService.showWarning("INCORRECT_SYMBOLS");
                return;
            }
        };

        $scope.deleteCheckedFiles = function () {
            var filesId = $scope.audioFiles.filter(file => file.checkedForDelete).map(file => file.id);

            $scope.deleteFilePromise = promiseCommonService.createPromise(
                examinationHttpService.deleteSelecedAudioPromise,
                filesId,
                "ERROR_DURING_DELETING_FILES",
                function (response) {
                    if (response.data === DeleteExaminationAudioResult.Error) {
                        alertService.showError("ERROR_DURING_DELETING_FILES");
                        return;
                    }

                    if (response.data === DeleteExaminationAudioResult.Using) {
                        alertService.showWarning("DELETING_FILES_USING_MESSAGE");
                        return;
                    }

                    alertService.showSuccess("DELETING_FILES_SUCCESS_MESSAGE");
                    $scope.fetchFiles();
                });
        };

        $scope.downloadAudioFile = function (file) {
            file.loadingFile = true;
            var linkToAudio = examinationHttpService.getAudioFileLink(file.id);
            createtWaveSurferPlayer(file);
            subscribeWavePlayerEvents(file);
            file.wavePlayer.load(linkToAudio);
        };

        $scope.closeAudioFile = function (file) {
            file.audioShow = false;
            file.wavePlayer.destroy();
        };

        $scope.playPauseAudioFile = function (file) {
            if (!file.wavePlayer.isPlaying()) {
                pauseAllAudios();
                file.playing = true;
                file.wavePlayer.play();
            }
            else {
                file.playing = false;
                file.wavePlayer.pause();
            }
        };

        $scope.stopAudioFile = function (file) {
            file.playing = false;
            file.wavePlayer.stop();
        };

        $scope.filterFiles = function (name) {
            if (name === "") {
                $scope.paginatedAudioFiles = $scope.audioFiles;
                $scope.setPage();
            }
            else if ($scope.audioFiles.length) {
                var data = [];
                $scope.audioFiles.forEach(function (item) {
                    if (item.name.toLowerCase().indexOf(name.toLowerCase()) !== -1) {
                        data.push(item);
                    }
                });
                $scope.paginatedAudioFiles = data;
            }
        };

        $scope.resetCheckForAllFiles = function () {
            $scope.isCheckedAllFiles = !$scope.isCheckedAllFiles;
            $scope.audioFiles.forEach(function (audioFile) {
                if ($scope.isCheckedAllFiles)
                    audioFile.checkedForDelete = true;
                else
                    audioFile.checkedForDelete = false;
            });
        };

        $scope.isAnyFileChecked = function () {
            return $scope.audioFiles.some(file => file.checkedForDelete);
        };

        $scope.confirmDelete = function () {
            $ngBootbox.confirm($translate.instant("DELETE_AUDIO_QUESTION_FILES_CONFIRMATION"))
                .then(function () {
                    $scope.deleteCheckedFiles();
                });
        };

        $scope.editAudioFile = function (file) {
            var modalWindowData = {
                checkFileName: $scope.checkIfFileExist,
                validateFileInfo: validateAudioFileInfo,
                onSave: updateAudioFileInfo,
                fileInfo: fillAudioFileInfo(file)
            };

            openModalWindow(modalWindowData);
        };

        $scope.checkIfFileExistOnServer = function (file) {
            if (file) {
                var request = { fileName: file };
                $scope.isFileExistsOnServer = false;
                $scope.blockWhileCheking = true;
                $scope.checkIfFileExistOnServerPromise = promiseCommonService.createPromise(
                    examinationHttpService.checkIfFileExistOnServerPromise,
                    request,
                    "ERROR_DURING_UPDATING_FILES",
                    function (response) {
                        $scope.blockWhileCheking = false;
                        $scope.isFileExistsOnServer = response.data;
                    });
            }
            else {
                $scope.isFileExistsOnServer = false;
            }
        };

        $scope.checkIfFileExist = function (fileName) {
            if (isAudioFileInfoNotCompleted(fileName) || hasInputNoizeCharacters(fileName))
                return;

            checkIfFileExistOnServer(fileName);
        };

        $('#audioFileInput').on('fileselect', function (event, numFiles, label) {
            $scope.audioFileInfo.fileName = label.substr(0, label.indexOf('.'));
            checkIfFileExistOnServer($scope.audioFileInfo.fileName);
            $scope.$apply();
        });

        var fetchAudioFiles = function (request) {
            $scope.fetchAudioFilesPromise = promiseCommonService.createPromise(
                examinationHttpService.getAllExaminationAudiosPromise,
                null,
                "ERROR_DURING_FETCHING_EXAMINATION_AUDIOS",
                function (response) {
                    if (response.data.length === 0) {
                        $scope.audioFiles = [];
                        alertService.showWarning("FILES_DONT_EXIST");
                        return;
                    }
                    else {
                        fillAudioFilesModel(response.data);
                    }
                });
        };

        var isQueryValid = function (query) {
            return query.length >= $scope.minAmountOfLetters;
        };

        var hasInputNoizeCharacters = function (inputString) {
            return !(/^[a-zA-Zа-яА-Я0-9\s]+$/.test(inputString) || inputString.length === 0);
        };

        var isInputEmpty = function (request) {
            return request.length === 0;
        };

        var isAudioFileInfoNotCompleted = function (inputInfo) {
            if (inputInfo === undefined || inputInfo === null)
                return true;

            return inputInfo.length < $scope.minAmountOfLetters;
        };

        var fillAudioFilesModel = function (data) {
            $scope.audioFiles = data.map(info => { return createAudioFileItem(info)});

            $scope.pagination.totalItems = data.length;

            $scope.isCheckedAllFiles = false;
            $scope.setPage();
        };

        var createAudioFileItem = function (file) {
            return {
                id: file.id,
                name: file.name,
                checkedForDelete: false,
                comment: file.comment,
                loadingFile: false,
                audioShow: false,
                playing: false
            };
        };

        var resetAudioFileInfo = function () {
            $scope.audioFileInfo = createAudioFileInfo();
            $scope.fileInfoModel = null;
        };

        function validateAudioFileInfo(fileInfo) {
            if (isAudioFileInfoNotCompleted(fileInfo.fileName) || fileInfo.categoryId === 0) {
                alertService.showWarning("FILL_ALL_GAPS");
                return false;
            }

            if (hasInputNoizeCharacters(fileInfo.fileName)) {
                alertService.showWarning("INCORRECT_SYMBOLS");
                return false;
            }
            return true;
        }

        function fillAudioFileInfo(file) {
            var fileData = createAudioFileInfo();

            fileData.id = file.id;
            fileData.fileName = file.name;
            fileData.comment = file.comment;

            return fileData;
        }

        function updateAudioFileInfo(fileInfo) {
            $scope.updateAudioFilesPromise = promiseCommonService.createPromise(
                examinationHttpService.updateAudioFileInfoPromise,
                fileInfo,
                "ERROR_DURING_CREATING_FILE",
                function (response) {
                    if (response.data) {
                        alertService.showSuccess("UPDATE_FILE_SUCCESS_MESSAGE");
                    }
                    else
                        alertService.showError("UPDATE_FILE_FAILED_MESSAGE");

                    $scope.fetchFiles();
                });
        }

        function createAudioFileInfo() {
            return {
                fileName: "",
                comment: ""
            };
        }

        function createtWaveSurferPlayer(file) {
            var elementId = "#waveAudioPlayer" + file.id;

            file.wavePlayer = WaveSurfer.create({
                container: elementId,
                waveColor: 'violet',
                progressColor: 'purple',
                height: 40
            });
        }

        function subscribeWavePlayerEvents(file) {
            file.wavePlayer.on('ready', function () {
                $scope.$apply(function () {
                    file.loadingFile = false;
                    file.audioShow = true;
                });
                file.wavePlayer.drawBuffer();
            });

            file.wavePlayer.on('finish', function () {
                $scope.$apply(function () {
                    file.playing = false;
                });
                file.wavePlayer.seekTo(0);
            });

            file.wavePlayer.on('error', function (message) {
                alertService.showError("ERROR_ON_FETCHING_AUDIO_FILE");
                $scope.$apply(function () {
                    file.loadingFile = false;
                    file.audioShow = false;
                });
            });
        }

        function pauseAllAudios() {
            for (var i = 0; i < $scope.audioFiles.length; i++) {
                if ($scope.audioFiles[i].wavePlayer !== undefined && $scope.audioFiles[i].wavePlayer.isPlaying()) {
                    $scope.audioFiles[i].playing = false;
                    $scope.audioFiles[i].wavePlayer.pause();
                }
            }
        }

        function openModalWindow(formData) {
            $uibModal.open({
                animation: true,
                templateUrl: 'templates/examination/audioFileModalTemplate.html',
                controller: 'audioFileModalController',
                resolve: {
                    modalData: formData
                }
            });
        }
    }
})();