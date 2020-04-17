(function () {
    'use strict';

    angular
        .module('Company.gazoo.labeling')
        .controller('transcriptionController', transcriptionController);

    angular
        .module('Company.gazoo.labeling')
        .component('transcription', {
            controller: 'transcriptionController',
            templateUrl: '/templates/labeling/transcription.html'
        });

    transcriptionController.$inject = ['$scope', 'transacriptionHttpService', 'promiseCommonService', '$ngBootbox', 'hotkeys', 'alertService', 'elapsedTimeService'];

    function transcriptionController($scope, transacriptionHttpService, promiseCommonService, $ngBootbox, hotkeys, alertService, elapsedTimeService) {
        $scope.transcriptionModel = {};
        $scope.assignedLabels = [];
        $scope.selectedLabelIds = [];
        $scope.authorFullName = null;
        $scope.workInProgress = false;
        var currentAction = null;

        $scope.playbackRateSelector = [
            { type: 1, name: 'x1' },
            { type: 0.75, name: 'x0.75' },
            { type: 0.5, name: 'x0.5' }
        ];

        $scope.selectedPlaybackRate;

        $scope.setPlayBackRate = function () {
            if ($scope.transcriptionModel.wavePlayer)
                $scope.transcriptionModel.wavePlayer.backend.setPlaybackRate($scope.selectedPlaybackRate);
        };

        $scope.startTranscription = function () {
            currentAction = LabelingAction.Labeling;
            $scope.fetchAudioPromise = promiseCommonService.createPromise(
                transacriptionHttpService.getAudioForTranscription,
                null,
                "ERROR",
                function (response) {
                    if (!response.data) {
                        alertService.showWarning("NO_DATA_FOR_TRASNCRIPTION_WARNING");
                        resetModel();
                        $scope.workInProgress = false;
                        return;
                    }
                    elapsedTimeService.startTimer();
                    setTranscriptionModel(response.data.transcription);
                    $scope.assignedLabels = response.data.transcriptionLabels;
                },
                function () {
                    resetModel();
                    $scope.workInProgress = false;
                });
        };

        $scope.startVerification = function () {
            currentAction = LabelingAction.Verification;
            $scope.fetchAudioPromise = promiseCommonService.createPromise(
                transacriptionHttpService.getAudioForVerification,
                null,
                "ERROR",
                function (response) {
                    if (!response.data) {
                        alertService.showWarning("NO_DATA_FOR_VERIFICATION_WARNING");
                        resetModel();
                        $scope.workInProgress = false;
                        return;
                    }
                    elapsedTimeService.startTimer();
                    setTranscriptionModel(response.data.transcription);
                    $scope.assignedLabels = response.data.transcriptionLabels;
                    $scope.selectedLabelIds = response.data.selectedLabelIds;
                    $scope.authorFullName = response.data.authorFullName;
                },
                function () {
                    resetModel();
                    $scope.workInProgress = false;
                });
        };

        $scope.nextAudio = function () {
            var request = {
                spentTime: elapsedTimeService.stopTimer(),
                transcription: $scope.transcriptionModel,
                selectedLabelIds: getSelectedLabelIds()
            };

            if (currentAction === LabelingAction.Labeling) {
                $scope.saveAudioTrancriptionPromise = promiseCommonService.createPromise(
                    transacriptionHttpService.saveAudioTrancription,
                    request,
                    "ERROR",
                    function (response) {
                        resetModel();
                        $scope.startTranscription();
                    });
            }
            else {
                $scope.saveAudioVerificationPromise = promiseCommonService.createPromise(
                    transacriptionHttpService.saveAudioVerification,
                    request,
                    "ERROR",
                    function (response) {
                        resetModel();
                        $scope.startVerification();
                    });
            }
        };

        $scope.cancelTranscription = function () {
            var request = {
                spentTime: elapsedTimeService.stopTimer(),
                id: $scope.transcriptionModel.id,
                action: currentAction
            };

            $scope.releaseAudioPromise = promiseCommonService.createPromise(
                transacriptionHttpService.releaseAudio,
                request,
                "ERROR",
                function (response) {
                    $scope.workInProgress = false;
                    resetModel();
                });
        };

        $scope.copyDeepSpeechToAgentTranscription = function () {
            $scope.transcriptionModel.agentTranscription = angular.copy($scope.transcriptionModel.deepSpeechTranscription);
        };

        $scope.playPauseAudioFile = function () {
            if (!$scope.transcriptionModel.wavePlayer.isPlaying()) {
                $scope.transcriptionModel.playing = true;
                $scope.transcriptionModel.wavePlayer.play();
            }
            else {
                $scope.transcriptionModel.playing = false;
                $scope.transcriptionModel.wavePlayer.pause();
            }
        };

        function getSelectedLabelIds() {
            var result = [];

            if ($scope.assignedLabels.length !== 0)
                $scope.assignedLabels.forEach(function (item) {
                    item.labels.forEach(function (label) {
                        if (label.isSelected)
                            result.push(label.id);
                    });
                });

            return result;
        }

        function downloadAudioFile() {
            var linkToAudio = transacriptionHttpService.getAudioFileLink($scope.transcriptionModel.audioId);
            createtWaveSurferPlayer();
            subscribeWavePlayerEvents();
            $scope.transcriptionModel.loadingFile = true;
            $scope.transcriptionModel.wavePlayer.load(linkToAudio);
        }

        function setTranscriptionModel(data) {
            $scope.workInProgress = true;
            $scope.transcriptionModel = data;
            downloadAudioFile();
        }

        function resetModel() {
            elapsedTimeService.stopTimer();
            if ($scope.transcriptionModel.wavePlayer)
                $scope.transcriptionModel.wavePlayer.destroy();
            $scope.transcriptionModel = {};
            $scope.assignedLabels = [];
            $scope.selectedLabelIds = [];
            $scope.authorFullName = null;
        }

        function createtWaveSurferPlayer() {
            var node = document.getElementById("waveAudioPlayer");
            while (node.firstChild) {
                node.removeChild(node.firstChild);
            }

            var elementId = "#waveAudioPlayer";

            $scope.transcriptionModel.wavePlayer = WaveSurfer.create({
                container: elementId,
                waveColor: 'violet',
                progressColor: 'purple',
                height: 40
            });
        }

        function subscribeWavePlayerEvents() {
            $scope.transcriptionModel.wavePlayer.on('ready', function () {
                var st = new window.soundtouch.SoundTouch(
                    $scope.transcriptionModel.wavePlayer.backend.ac.sampleRate
                );
                var buffer = $scope.transcriptionModel.wavePlayer.backend.buffer;
                var channels = buffer.numberOfChannels;
                var l = buffer.getChannelData(0);
                var r = channels > 1 ? buffer.getChannelData(1) : l;
                var length = buffer.length;
                var seekingPos = null;
                var seekingDiff = 0;

                var source = {
                    extract: function (target, numFrames, position) {
                        if (seekingPos !== null) {
                            seekingDiff = seekingPos - position;
                            seekingPos = null;
                        }

                        position += seekingDiff;

                        for (var i = 0; i < numFrames; i++) {
                            target[i * 2] = l[i + position];
                            target[i * 2 + 1] = r[i + position];
                        }

                        return Math.min(numFrames, length - position);
                    }
                };

                var soundtouchNode;

                $scope.transcriptionModel.wavePlayer.on('play', function () {
                    seekingPos = ~~($scope.transcriptionModel.wavePlayer.backend.getPlayedPercents() * length);
                    st.tempo = $scope.transcriptionModel.wavePlayer.getPlaybackRate();

                    if (st.tempo === 1) {
                        $scope.transcriptionModel.wavePlayer.backend.disconnectFilters();
                    } else {
                        if (!soundtouchNode) {
                            var filter = new window.soundtouch.SimpleFilter(source, st);
                            soundtouchNode = window.soundtouch.getWebAudioNode(
                                $scope.transcriptionModel.wavePlayer.backend.ac,
                                filter
                            );
                        }
                        $scope.transcriptionModel.wavePlayer.backend.setFilter(soundtouchNode);
                    }
                });

                $scope.transcriptionModel.wavePlayer.on('pause', function () {
                    soundtouchNode && soundtouchNode.disconnect();
                });

                $scope.transcriptionModel.wavePlayer.on('seek', function () {
                    seekingPos = ~~($scope.transcriptionModel.wavePlayer.backend.getPlayedPercents() * length);
                });

                $scope.$apply(function () {
                    $scope.selectedPlaybackRate = "1";
                    $scope.transcriptionModel.loadingFile = false;
                    $scope.transcriptionModel.audioShow = true;
                    window.addEventListener('resize', () => {
                        $scope.transcriptionModel.wavePlayer.drawer.containerWidth = $scope.transcriptionModel.wavePlayer.drawer.container.clientWidth;
                        $scope.transcriptionModel.wavePlayer.drawBuffer();
                    });
                });

                $scope.transcriptionModel.wavePlayer.drawBuffer();
                document.getElementById("agentTranscriptionTextarea").focus(); 
            });

            $scope.transcriptionModel.wavePlayer.on('finish', function () {
                $scope.$apply(function () {
                    $scope.transcriptionModel.playing = false;
                });
                $scope.transcriptionModel.wavePlayer.seekTo(0);
            });

            $scope.transcriptionModel.wavePlayer.on('error', function (message) {
                $scope.$apply(function () {
                    $scope.cancelTranscription();
                    $scope.transcriptionModel.loadingFile = false;
                    $scope.transcriptionModel.audioShow = false;
                    $scope.workInProgress = false;
                });
            });

            $scope.transcriptionModel.wavePlayer.on('destroy', function () {
                window.removeEventListener('resize', () => {
                    $scope.transcriptionModel.wavePlayer.drawer.containerWidth = $scope.transcriptionModel.wavePlayer.drawer.container.clientWidth;
                    $scope.transcriptionModel.wavePlayer.drawBuffer();
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

        hotkeys.add({
            combo: 'f4',
            description: 'copy',
            allowIn: ['INPUT', 'TEXTAREA'],
            callback: function () {
                $scope.copyDeepSpeechToAgentTranscription();
            }
        });

        hotkeys.add({
            combo: 'ctrl+alt',
            description: 'skip backward',
            allowIn: ['INPUT', 'TEXTAREA'],
            callback: function () {
                if ($scope.transcriptionModel.wavePlayer) {
                    $scope.transcriptionModel.wavePlayer.skipBackward(2);
                }
            }
        });
    }
})();