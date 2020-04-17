(() => {
    'use strict';

    angular
        .module('Company.gazoo.labeling')
        .component('recordingPanel', {
            controller: 'recordingPanelController',
            templateUrl: '/templates/labeling/recordingPanel.html',
            bindings: {
                recordingId: "<"
            }
        });

    angular
        .module('Company.gazoo.labeling')
        .controller('recordingPanelController', recordingPanelController);

    recordingPanelController.$inject = ['$scope', 'promiseCommonService', 'alertService', 'recordingLabelingService', 'transacriptionHttpService', 'recordingSharedService', 'mapService'];

    function recordingPanelController($scope, promiseCommonService, alertService, recordingLabelingService, transacriptionHttpService, recordingSharedService, mapService) {
        $scope.pageIsReady = false;
        $scope.call = {};
        $scope.labelGroups = [];
        $scope.callReady = false;
        $scope.callAudioStatus = CallAudioStatus;
        $scope.getCallAudioStatus = (id) => { return mapService.getInvertedMap(CallAudioStatus)[id]; };
        $scope.setPageIsReady = () => { $scope.pageIsReady = true; };
        var updatedMessageLogs = [],
            clientPlayerContainerId = 'clientWaveAudioPlayer',
            agentPlayerContainerId = 'agentWaveAudioPlayer';

        $scope.initialize = () => {
            if (this.recordingId === undefined || !this.recordingId || isNaN(parseInt(this.recordingId)))
                alertService.showError("ERROR_INVALID_RECORDING_ID");
            else
                prepareData();
        };

        function prepareData() {
            getLabelGroups()
                .then(getMessageLogs)
                .then(getCallData)
                .then(getTranscription)
                .then(() => {
                    $scope.callReady = $scope.call.agentChannel.ready && $scope.call.clientChannel.ready;
                    if ($scope.call.info.status !== CallAudioStatus.Synced) {
                        $scope.callReady = false;
                        alertService.showError("ERROR_AUDIO_IS_NOT_SYNCED");
                    }

                    if (!$scope.callReady)
                        alertService.showError("ERROR_DURING_FETCHING_CHANNELS_AUDIO_DATA");
                });
        }

        function getLabelGroups() {
            return promiseCommonService.createPromise(transacriptionHttpService.getLabelGroups, null, "ERROR_DURING_GETTING_RECORDING_REPORT", (response) => {
                if (!response) {
                    alertService.showError("ERROR_DURING_GETTING_RECORDING_REPORT");
                    return;
                }
                $scope.labelGroups = response.data;
                $scope.labelGroups.forEach((group) => {
                    getLabels(group);
                });
            });
        }

        function getLabels(group) {
            return promiseCommonService.createPromise(transacriptionHttpService.getLabels, group.id, "ERROR", (response) => {
                group.options = response.data;
            });
        };

        let getMessageLogs = () => {
            return promiseCommonService.createPromise(recordingLabelingService.getAudioMessageLogUpdates, this.recordingId, "ERROR_DURING_GETTING_RECORDING_REPORT", (response) => {
                if (!response) {
                    alertService.showError("ERROR_DURING_GETTING_RECORDING_REPORT");
                    return;
                }
                updatedMessageLogs = response.data;
            });
        }

        let getCallData = () => {
            return promiseCommonService.createPromise(recordingSharedService.getCallDetail, this.recordingId, "ERROR_DURING_GETTING_RECORDING_REPORT", (response) => {
                if (!response) {
                    alertService.showError("ERROR_DURING_GETTING_RECORDING_REPORT");
                    return;
                }
                if (response.data.totalCount === 0) {
                    alertService.showWarning("TOOLTIP_NO_RECORDING_MESSAGE");
                }
                $scope.call.info = getReceivedRecordings(response.data);
                $scope.call.agentChannel = prepareChannelData(response.data.audioMessageLogs, ChannelType.Agent);
            });
        }

        let getTranscription = () => {
            return promiseCommonService.createPromise(recordingLabelingService.getClientUtterances, this.recordingId, "ERROR_DURING_GETTING_RECORDING_REPORT", (response) => {
                if (!response) {
                    alertService.showError("ERROR_DURING_GETTING_RECORDING_REPORT");
                    return;
                }
                $scope.call.clientChannel = prepareChannelData(response.data, ChannelType.Client);
                $scope.callReady = true;
            });
        }

        let getFileLink = (channelType) => {
            return recordingSharedService.getAudioChannelLink(this.recordingId, channelType);
        }

        function getSelectedLabels(selectedLabels) {
            var labels = $scope.labelGroups.map(group => ({ id: group.id, values: [] }));
            selectedLabels.forEach(selectedLabelId => {
                var groupId = $scope.labelGroups.find(group => group.options.some(label => label.id === selectedLabelId)).id;
                labels.find(label => label.id === groupId).values.push(selectedLabelId);
            });
            return labels;
        }

        function prepareChannelData(data, channelType) {
            return {
                "type": channelType,
                "audioFileLink": getFileLink(channelType),
                "containerId": channelType === ChannelType.Client ? clientPlayerContainerId : agentPlayerContainerId,
                "logs": prepareLogs(data, channelType),
                "currentLog": [],
                "currentTime": '0.000',
                "ready": true,
                "playing": false,
                "muted": false,
                "loadingFile": false
            };
        }

        function getReceivedRecordings(data) {
            var recording = data.callAudioReport;
            var startTime = moment(recording.startTime).local();
            var endTime = moment(recording.endTime).local();
            var duration = moment.duration(endTime.diff(startTime));

            return {
                "callAudioId": recording.callAudioId,
                "agentId": recording.agentId,
                "campaignId": recording.campaignId,
                "phoneNumber": recording.phoneNumber,
                "status": recording.status,
                "businessResultName": recording.businessResultName,
                "agentName": recording.agentName,
                "startTime": startTime,
                "endTime": endTime,
                "duration": duration,
                "startTimeFormat": moment.utc(recording.startTime).local().format("YYYY-MM-DD HH:mm:ss"),
                "durationFormat": duration.format(),
                "transferTime": data.transferTime
            };
        }

        function prepareLogs(logs, channelType) {
            if (channelType === ChannelType.Client)
                return logs.map(log => (
                    {
                        ...log,
                        startTimeFormat: moment.duration(log.startTime, 'seconds').format("mm:ss.SSS", { trim: false }),
                        endTimeFormat: moment.duration(log.endTime, 'seconds').format("mm:ss.SSS", { trim: false }),
                        labels: getSelectedLabels(log.selectedLabels),
                    }
                ));
            else {
                return logs.map(log => {
                    var updatedLog = updatedMessageLogs.find(updatedMessage => updatedMessage.id === log.id),
                        startTime = updatedLog ? updatedLog.startTime : log.startPlayingTime,
                        endTime = updatedLog ? updatedLog.endTime : (log.startPlayingTime + log.playedDuration);
                    return {
                        id: log.id,
                        audioCompleteness: Math.round(log.playedDuration / log.fileDuration * 100),
                        audioName: log.audioName,
                        isBotAgent: log.isBotAgent,
                        startTime: startTime,
                        startTimeFormat: moment.duration(startTime, 'seconds').format("mm:ss.SSS", { trim: false }),
                        endTime: endTime,
                        endTimeFormat: moment.duration(endTime, 'seconds').format("mm:ss.SSS", { trim: false })
                    }
                });
            }
        }
    }
})();
