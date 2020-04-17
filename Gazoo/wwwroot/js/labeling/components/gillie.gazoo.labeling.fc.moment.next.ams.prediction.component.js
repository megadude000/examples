(function () {
    'use strict';

    angular
        .module('Company.gazoo.labeling')
        .controller('nextAmsPredictionController', nextAmsPredictionController);

    angular
        .module('Company.gazoo.labeling')
        .component('nextAmsPrediction', {
            controller: 'nextAmsPredictionController',
            templateUrl: '/templates/labeling/nextAmsPrediction.html'
        });

    nextAmsPredictionController.$inject = ['$scope', 'fcMomentHttpService', 'promiseCommonService', '$ngBootbox', 'hotkeys', 'alertService', 'elapsedTimeService'];

    function nextAmsPredictionController($scope, fcMomentHttpService, promiseCommonService, $ngBootbox, hotkeys, alertService, elapsedTimeService) {
        var currentAction = null;
        $scope.authorFullName = null;
        $scope.nextAmsPredictionModel = {};
        $scope.workInProgress = false;
        $scope.selectSettings = {
            idProp: 'audioMessage',
            displayProp: 'audioMessage',
            template: '{{option.audioMessage}} ({{option.confidence}})',
            scrollableHeight: '400px',
            scrollable: true,
            enableSearch: true,
            showCheckAll: false
        };
        $scope.audioMessageLogs = [];
        $scope.possibleAms = [];
        $scope.selectedAms = [];

        $scope.startNextAmsPrediction = function () {
            currentAction = LabelingAction.Labeling;
            $scope.fetchAudioPromise = promiseCommonService.createPromise(
                fcMomentHttpService.getAudioForNextAmsPrediction,
                null,
                "ERROR",
                function (response) {
                    if (!response.data) {
                        alertService.showWarning("NO_DATA_FOR_AMS_PREDICTION_WARNING");
                        resetModel();
                        $scope.workInProgress = false;
                        return;
                    }
                    elapsedTimeService.startTimer();
                    setNextAmsPredictionModel(response.data.nextAmsPredictionModel);
                    $scope.audioMessageLogs = response.data.audioMessageLogs;
                },
                function () {
                    resetModel();
                    $scope.workInProgress = false;
                    $scope.assignedLabels = [];
                });
        };

        $scope.startNextAmsVerification = () => {
            currentAction = LabelingAction.Verification;
            $scope.fetchAudioPromise = promiseCommonService.createPromise(
                fcMomentHttpService.getAudioForNextAmsVerification,
                null,
                "ERROR",
                function (response) {
                    if (!response.data) {
                        alertService.showWarning("NO_DATA_FOR_AMS_VERIFICATION_WARNING");
                        resetModel();
                        $scope.workInProgress = false;
                        return;
                    }
                    elapsedTimeService.startTimer();
                    setNextAmsPredictionModel(response.data.nextAmsPredictionModel);
                    $scope.audioMessageLogs = response.data.audioMessageLogs;
                    $scope.authorFullName = response.data.authorFullName;
                },
                function () {
                    $scope.workInProgress = false;
                    $scope.assignedLabels = [];
                });
        };

        $scope.nextAudio = function () {
            var request = {
                spentTime: elapsedTimeService.stopTimer(),
                id: $scope.nextAmsPredictionModel.id,
                bestAnswerTime: parseFloat($scope.nextAmsPredictionModel.wavePlayer.getCurrentTime()),
                isPerfect: $scope.nextAmsPredictionModel.isPerfect,
                result: formResult()
            };

            if (currentAction === LabelingAction.Labeling) {
                $scope.saveResultPromise = promiseCommonService.createPromise(
                    fcMomentHttpService.saveMomentPredictionResult,
                    request,
                    "ERROR",
                    function (response) {
                        resetModel();
                        $scope.startNextAmsPrediction();
                    });
            }
            else {
                $scope.saveResultPromise = promiseCommonService.createPromise(
                    fcMomentHttpService.saveMomentVerificationResult,
                    request,
                    "ERROR",
                    function (response) {
                        resetModel();
                        $scope.startNextAmsVerification();
                    });
            }
        };

        function formResult() {
            var selectedAms = [];
            $scope.selectedAms.forEach(function (entry) {
                var selected = {
                    audioName: entry.id,
                    orderNumber: $scope.selectedAms.indexOf(entry),
                    waitAnswer: entry.waitAnswer.toString()
                };
                selectedAms.push(selected);
            });

            return JSON.stringify(selectedAms);
        }

        $scope.onDrop = function (srcList, srcIndex, targetList, targetIndex) {
            if (srcList !== targetList)
                return false;

            targetList.splice(targetIndex, 0, srcList[srcIndex]);
            if (srcList === targetList && targetIndex <= srcIndex)
                srcIndex++;
            srcList.splice(srcIndex, 1);
            return true;
        };

        function setAudioCurrentTime() {
            $scope.nextAmsPredictionModel.currentTime = $scope.nextAmsPredictionModel.wavePlayer.getCurrentTime().toFixed(2);
        }

        function setSelectedAms(data) {
            var tempArray = [];
            var parsedResult = JSON.parse(data.selectedAms);
            parsedResult.map((entry) => {
                tempArray.push({
                    id: entry.audioName,
                    waitAnswer: entry.waitAnswer === "true"
                });
            });
            $scope.selectedAms = tempArray;
        }

        function setNextAmsPredictionModel(data) {
            $scope.workInProgress = true;
            $scope.nextAmsPredictionModel = data;
            $scope.possibleAms = JSON.parse(data.possibleAms);
            $scope.possibleAms.sort((a, b) => parseFloat(a.confidence) < parseFloat(b.confidence));

            if (data.selectedAms)
                setSelectedAms(data);

            downloadAudioFile();
        }

        function downloadAudioFile() {
            var linkToAudio = fcMomentHttpService.getAudioFileLink($scope.nextAmsPredictionModel.audioId);
            createtWaveSurferPlayer();
            subscribeWavePlayerEvents();
            $scope.nextAmsPredictionModel.loadingFile = true;
            $scope.nextAmsPredictionModel.wavePlayer.load(linkToAudio);
        }

        function createtWaveSurferPlayer() {
            var node = document.getElementById("amsPredictionWaveAudioPlayer");
            while (node.firstChild) {
                node.removeChild(node.firstChild);
            }

            var elementId = "#amsPredictionWaveAudioPlayer";

            $scope.nextAmsPredictionModel.wavePlayer = WaveSurfer.create({
                container: elementId,
                waveColor: 'violet',
                progressColor: 'purple',
                height: 40
            });
        }

        function subscribeWavePlayerEvents() {
            $scope.nextAmsPredictionModel.wavePlayer.on('ready', function () {
                var position = null;

                $scope.$apply(function () {
                    $scope.nextAmsPredictionModel.loadingFile = false;
                    $scope.nextAmsPredictionModel.audioShow = true;
                    addAudioMessageLogs();
                    if (!$scope.nextAmsPredictionModel.bestAnswerTime)
                        $scope.nextAmsPredictionModel.currentTime = '00:00';
                    else {
                        var duration = $scope.nextAmsPredictionModel.wavePlayer.getDuration();
                        var selectedTime = moment.duration($scope.nextAmsPredictionModel.bestAnswerTime).asSeconds();
                        position = selectedTime / duration;
                    }
                    window.addEventListener('resize', () => {
                        $scope.nextAmsPredictionModel.wavePlayer.drawer.containerWidth = $scope.nextAmsPredictionModel.wavePlayer.drawer.container.clientWidth;
                        $scope.nextAmsPredictionModel.wavePlayer.drawBuffer();
                    });
                });

                $scope.nextAmsPredictionModel.wavePlayer.drawBuffer();

                if (position) {
                    $scope.nextAmsPredictionModel.wavePlayer.seekTo(position);
                    $scope.nextAmsPredictionModel.wavePlayer.drawer.progress(position);
                }

            });

            $scope.nextAmsPredictionModel.wavePlayer.on('finish', function () {
                $scope.$apply(function () {
                    $scope.nextAmsPredictionModel.playing = false;
                });
                $scope.nextAmsPredictionModel.wavePlayer.seekTo(0);
            });

            $scope.nextAmsPredictionModel.wavePlayer.on('seek', () => {
                $scope.$applyAsync(() => {
                    setAudioCurrentTime();
                });
            });

            $scope.nextAmsPredictionModel.wavePlayer.on('audioprocess', () => {
                $scope.$applyAsync(() => {
                    setAudioCurrentTime();
                });
            });

            $scope.nextAmsPredictionModel.wavePlayer.on('error', function (message) {
                alertService.showError("ERROR_ON_FETCHING_AUDIO_FILE");
                $scope.$apply(function () {
                    resetModel();
                    $scope.workInProgress = false;
                    $scope.nextAmsPredictionModel.loadingFile = false;
                    $scope.nextAmsPredictionModel.audioShow = false;
                    $scope.cancelAmsPrediction();
                });
            });

            $scope.nextAmsPredictionModel.wavePlayer.on('destroy', function () {
                window.removeEventListener('resize', () => {
                    $scope.nextAmsPredictionModel.wavePlayer.drawer.containerWidth = $scope.nextAmsPredictionModel.wavePlayer.drawer.container.clientWidth;
                    $scope.nextAmsPredictionModel.wavePlayer.drawBuffer();
                });
            });
        }

        hotkeys.add({
            combo: 'f2',
            description: 'play/pause',
            allowIn: ['INPUT', 'TEXTAREA'],
            callback: function () {
                $scope.playPauseAudioFile();
            }
        });

        hotkeys.add({
            combo: 'ctrl+enter',
            description: 'save',
            allowIn: ['INPUT', 'TEXTAREA'],
            callback: function () {
                $scope.nextAudio();
            }
        });

        $scope.playPauseAudioFile = function () {
            if (!$scope.nextAmsPredictionModel.wavePlayer.isPlaying()) {
                $scope.nextAmsPredictionModel.playing = true;
                $scope.nextAmsPredictionModel.wavePlayer.play();
            }
            else {
                $scope.nextAmsPredictionModel.playing = false;
                $scope.nextAmsPredictionModel.wavePlayer.pause();
            }
        };

        $scope.cancelAmsPrediction = function () {
            var request = {
                spentTime: elapsedTimeService.stopTimer(),
                id: $scope.nextAmsPredictionModel.id,
                action: currentAction
            };

            $scope.releaseAudioPromise = promiseCommonService.createPromise(
                fcMomentHttpService.releaseAudio,
                request,
                "ERROR",
                function (response) {
                    $scope.workInProgress = false;
                    resetModel();
                });
        };

        function addAudioMessageLogs() {
            if ($scope.audioMessageLogs.length) {
                $scope.audioMessageLogs.forEach((log) => {
                    log.audioCompleteness = Math.round(log.completeness * 100);
                });
            }
        }

        function resetModel() {
            elapsedTimeService.stopTimer();
            if ($scope.nextAmsPredictionModel.wavePlayer)
                $scope.nextAmsPredictionModel.wavePlayer.destroy();
            $scope.nextAmsPredictionModel = {};
            $scope.audioMessageLogs = [];
            $scope.possibleAms = [];
            $scope.selectedAms = [];
            $scope.authorFullName = null;
        }
    }
})();