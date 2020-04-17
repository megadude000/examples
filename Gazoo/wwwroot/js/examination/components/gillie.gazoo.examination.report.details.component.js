(function () {
    'use strict';

    angular
        .module('Company.gazoo.examination')
        .controller('reportDetailsController', reportDetailsController);

    angular
        .module('Company.gazoo.examination')
        .component('reportDetails', {
            controller: 'reportDetailsController',
            templateUrl: '/templates/examination/reportDetails.html',
            bindings: {
                reportId: "<"
            }
        });

    reportDetailsController.$inject = ['$scope', 'promiseCommonService', 'examinationHttpService', 'orderByFilter', 'alertService'];

    function reportDetailsController($scope, promiseCommonService, examinationHttpService, orderByFilter, alertService) {
        var self = this;
        $scope.propertyName = 'answerResult';
        $scope.reverse = false;
        $scope.examinationReport = {};

        $scope.initialize = function () {
            $scope.fetchExaminationReport(self.reportId);
        };

        $scope.fetchExaminationReport = function (reportId) {
            $scope.fetchExaminationPromise = promiseCommonService.createPromise(
                examinationHttpService.getExaminationReport,
                reportId,
                "ERROR_DURING_FETCHING_EXAMINATION_REPORT",
                function (res) {
                    $scope.examinationReport = res.data;
                    $scope.examinationReport.questionResults.forEach(function (entry) {
                        entry.loadingFile = false;
                        entry.audioShow = false;
                        entry.playing = false;
                    });
                });
        };

        $scope.sortBy = function (propertyName) {
            $scope.reverse = ($scope.propertyName === propertyName) ? !$scope.reverse : false;
            $scope.propertyName = propertyName;
            $scope.examinationReport.questionResults = orderByFilter($scope.examinationReport.questionResults, $scope.propertyName, $scope.reverse);
        };

        $scope.GetTypeAsString = function (type) {
            if (type === 1)
                return 'SCRIPT_LABEL';
            else if (type === 2)
                return 'OBJECTION_LABEL';
            else if (type === 3)
                return 'QUICK_ANSWER_LABEL';
            else
                return 'NO_ANSWER_WAS_GIVEN';
        };

        $scope.GetResultTypeAsString = function (type) {
            if (type === 1)
                return 'CORRECT_ANSWER_LABEL';
            else if (type === 2)
                return 'INCORRECT_ANSWER_LABEL';
            else if (type === 3)
                return 'TIMEUP_ANSWER_LABEL';
            else
                return 'STOPPED_LABEL';
        };

        $scope.downloadAudioFile = function (file) {
            file.loadingFile = true;
            var linkToAudio = examinationHttpService.getAudioFileLink(file.question.questionAudioFileId);
            createtWaveSurferPlayer(file);
            subscribeWavePlayerEvents(file);
            file.wavePlayer.load(linkToAudio);
        };

        function createtWaveSurferPlayer(file) {
            var elementId = "#resultWaveAudioPlayer" + file.question.questionAudioFileId;

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

        function pauseAllAudios() {
            for (var i = 0; i < $scope.examinationReport.questionResults.length; i++) {
                if ($scope.examinationReport.questionResults[i].wavePlayer !== undefined && $scope.examinationReport.questionResults[i].wavePlayer.isPlaying()) {
                    $scope.examinationReport.questionResults[i].playing = false;
                    $scope.examinationReport.questionResults[i].wavePlayer.pause();
                }
            }
        }
    }
})();