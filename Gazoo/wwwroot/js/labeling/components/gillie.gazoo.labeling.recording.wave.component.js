(function () {
    'use strict';

    angular
        .module('Company.gazoo.labeling')
        .component('recordingWave', {
            controller: 'recordingWaveController',
            templateUrl: '/templates/labeling/recordingWave.html',
            bindings: {
                call: "<",
                labelGroups: "<",
                onComponentLoaded: "&"
            }
        });

    angular
        .module('Company.gazoo.labeling')
        .controller('recordingWaveController', recordingWaveController);

    recordingWaveController.$inject = ['$scope', 'promiseCommonService', 'alertService', 'recordingLabelingService', 'mapService', 'hotkeys'];

    function recordingWaveController($scope, promiseCommonService, alertService, recordingLabelingService, mapService, hotkeys) {
        $scope.call = null;
        $scope.labels = [];
        $scope.enum = {
            genderTypes: mapService.getArrayFromMap(mapService.getInvertedMap(Gender)),
            utteranceType: mapService.getArrayFromMap(mapService.getInvertedMap(UtteranceType))
        };
        $scope.playbackRate = 1;
        $scope.options = {
            format: 'rgb',
            alpha: true,
            round: true,
            case: 'lower',
            swatchBootstrap: false,
            swatchOnly: true,
            inputClass: 'form-control'
        };

        var saveTimeAccuracy = 6,
            playerHeightPx = 150,
            currentScrollPosition = 0,
            zoom = {
                default: 0,
                current: 0,
                step: 50,
                timeOut: 100
            },
            playback = {
                default: 1,
                min: 0.1,
                max: 2,
                step: 0.1
            },
            timelinePosition = {
                min: 0,
                max: 1
            },
            seekToTime = {
                default: 0,
                skipStep: 5
            },
            color = {
                wave: 'rgba(95,158,160, 0.5)',
                progress: 'rgba(51, 122, 183, 0.5)',
                clientRegion: 'rgba(127, 191, 63, 0.5)',
                agentRegion: 'rgba(255, 255, 0, 0.5)',
                botRegion: 'rgba(255, 51, 0, 0.5)'
            };

        $scope.initialize = () => {
            $scope.call = this.call;
            $scope.labels = this.labelGroups;
            if ($scope.call.info.status === CallAudioStatus.Synced) {
                prepareWaveSurfer($scope.call);
                this.onComponentLoaded();
            }
        };

        $scope.playPauseToggle = () => {
            $scope.call.agentChannel.wavePlayer.playPause();
        };

        $scope.stopButton = () => {
            $scope.call.agentChannel.wavePlayer.stop();
        };

        $scope.muteChannel = (channelType) => {
            var channel = getChannel(channelType);
            channel.wavePlayer.toggleMute();
            channel.muted = channel.wavePlayer.getMute();
        };

        $scope.seekToSecond = (time) => {
            var newPosition = time / $scope.call.agentChannel.wavePlayer.getDuration();
            newPosition = newPosition > timelinePosition.max ? timelinePosition.max : (newPosition < timelinePosition.min ? timelinePosition.min : newPosition);
            $scope.call.agentChannel.wavePlayer.seekTo(newPosition);
        };

        $scope.setPlaybackRate = (rate) => {
            if ($scope.playbackRate < playback.min)
                $scope.playbackRate = playback.min;
            else if ($scope.playbackRate > playback.max)
                $scope.playbackRate = playback.max;

            if (rate === undefined)
                rate = $scope.playbackRate;

            $scope.call.agentChannel.wavePlayer.setPlaybackRate(rate);
            $scope.call.clientChannel.wavePlayer.setPlaybackRate(rate);
        }

        $scope.deleteRegion = (id) => {
            var index = $scope.call.clientChannel.logs.findIndex(log => log.id === id);
            if (index > -1) {
                $scope.call.clientChannel.logs.splice(index, 1);
                for (const [regionId, region] of Object.entries($scope.call.clientChannel.wavePlayer.regions.list)) {
                    if (Number(regionId) === id)
                        region.remove();
                }
            }
        };

        $scope.playRegion = (id, channelType) => {
            var channel = getChannel(channelType);
            for (const [regionId, region] of Object.entries(channel.wavePlayer.regions.list)) {
                if (Number(regionId) === id)
                    region.play();
            }
        };

        $scope.saveTranscription = () => {
            var request = {
                callId: $scope.call.info.callAudioId,
                clientUtterances: $scope.call.clientChannel.logs.map(log => (
                    {
                        startTime: log.startTime.toFixed(saveTimeAccuracy),
                        endTime: log.endTime.toFixed(saveTimeAccuracy),
                        selectedLabels: log.labels.map(label => label.values).flat(1)
                    })),
                audioMessageLogsUpdate: $scope.call.agentChannel.logs.map(log => (
                    {
                        id: log.id,
                        startTime: log.startTime.toFixed(saveTimeAccuracy),
                        endTime: log.endTime.toFixed(saveTimeAccuracy)
                    }))
            };

            promiseCommonService.createPromise(recordingLabelingService.updateCallLabelingAsync, request, "ERROR", (response) => {
                if (!response) {
                    alertService.showError("ERROR");
                    return;
                }
                else
                    alertService.showSuccess("SUCCESS");
            });
        }

        $scope.onChange = (logId, labelGroupId, values) => {
            $scope.call.clientChannel
                .logs.find(log => log.id === logId)
                .labels.find(label => label.id === labelGroupId)
                .values = values;
        };

        $scope.fileNameForSave = (channelType) => {
            return "recordingId_" + $scope.call.info.callAudioId + "_" + (channelType === ChannelType.Agent ? "agent_channel" : "client_channel") + ".wav";
        }

        function prepareWaveSurfer(channels) {
            createWaveSurferPlayer(channels.clientChannel);
            subscribeWavePlayerEvents(channels.clientChannel);
            channels.clientChannel.wavePlayer.load(channels.clientChannel.audioFileLink);

            createWaveSurferPlayer(channels.agentChannel);
            subscribeWavePlayerEvents(channels.agentChannel);
            channels.agentChannel.wavePlayer.load(channels.agentChannel.audioFileLink);
        }

        function createWaveSurferPlayer(channel) {
            if (document.getElementById(channel.containerId).childElementCount === 0)
                channel.wavePlayer = WaveSurfer.create({
                    container: '#' + channel.containerId,
                    waveColor: color.wave,
                    progressColor: color.progress,
                    responsive: true,
                    hideScrollbar: channel.type === ChannelType.Client,
                    height: playerHeightPx
                });
        }

        function getChannel(channelType) {
            return channelType === ChannelType.Agent ? $scope.call.agentChannel : $scope.call.clientChannel;
        }

        function getReverseChannel(channelType) {
            return channelType === ChannelType.Agent ? $scope.call.clientChannel : $scope.call.agentChannel;
        }

        function wheelZoomHandler(e) {
            var deltaY = 0;
            var zoomTimeOut = null;
            e.preventDefault();
            if (e.deltaY) {
                deltaY = e.deltaY;
            } else if (e.wheelDelta) {
                deltaY = -e.wheelDelta;
            }
            clearTimeout(zoomTimeOut);
            if (deltaY < 0) {
                zoom.current += zoom.step;
                zoomTimeOut = setTimeout(() => {
                    $scope.call.agentChannel.wavePlayer.zoom(zoom.current);
                    $scope.call.clientChannel.wavePlayer.zoom(zoom.current);
                }, zoom.timeOut);
            } else {
                if (zoom.current > zoom.default) {
                    zoom.current -= zoom.step;
                    if (zoom.current === zoom.default || zoom.current < zoom.default) {
                        $scope.call.agentChannel.wavePlayer.zoom(zoom.default);
                        $scope.call.clientChannel.wavePlayer.zoom(zoom.default);

                    }
                    else {
                        zoomTimeOut = setTimeout(() => {
                            $scope.call.agentChannel.wavePlayer.zoom(zoom.current);
                            $scope.call.clientChannel.wavePlayer.zoom(zoom.current);
                        }, zoom.timeOut);
                    }
                } else {
                    zoom.current = zoom.default;
                    $scope.call.agentChannel.wavePlayer.zoom(zoom.default);
                    $scope.call.clientChannel.wavePlayer.zoom(zoom.default);
                }
            }
        };

        function subscribeWavePlayerEvents(channel) {
            channel.wavePlayer.on('ready', () => {
                channel.loadingFile = false;
                channel.wavePlayer.container.addEventListener('wheel', wheelZoomHandler);
                zoom.default = zoom.current = Math.trunc(channel.wavePlayer.drawer.getWidth() / channel.wavePlayer.getDuration() / channel.wavePlayer.params.pixelRatio);
                if (channel.type === ChannelType.Client)
                    channel.wavePlayer.enableDragSelection(
                        {
                            drag: true,
                            resize: true
                        });
                loadRegions(channel.type);
                channel.wavePlayer.drawBuffer();
            });

            channel.wavePlayer.on('play', () => {
                var secondChannel = getReverseChannel(channel.type);
                var currentTime = channel.wavePlayer.getCurrentTime();
                var scheduledPause = channel.wavePlayer.backend.scheduledPause;

                if (!secondChannel.wavePlayer.isPlaying() || secondChannel.wavePlayer.getCurrentTime() !== currentTime) {
                    secondChannel.wavePlayer.play(currentTime, scheduledPause);
                }

                channel.playing = secondChannel.playing = true;
            });

            channel.wavePlayer.on('pause', () => {
                var secondChannel = getReverseChannel(channel.type);
                if (secondChannel.wavePlayer.isPlaying())
                    secondChannel.wavePlayer.pause();

                channel.playing = secondChannel.playing = false;
            });

            channel.wavePlayer.on('seek', (position) => {
                var secondChannel = getReverseChannel(channel.type);
                var currentTime = channel.wavePlayer.getCurrentTime();

                channel.currentTime = currentTime.toFixed(3);
                if (channel.logs)
                    channel.currentLog = getCurrentRegionIds(channel);

                if (currentTime !== secondChannel.wavePlayer.getCurrentTime())
                    secondChannel.wavePlayer.seekTo(position);
            });

            channel.wavePlayer.on('finish', () => {
                var secondChannel = getReverseChannel(channel.type);
                channel.playing = secondChannel.playing = false;
                channel.wavePlayer.seekTo(seekToTime.default);
            });

            channel.wavePlayer.on('scroll', () => {
                var secondChannel = getReverseChannel(channel.type);
                currentScrollPosition = channel.wavePlayer.drawer.wrapper.scrollLeft;

                if (secondChannel.wavePlayer.drawer.wrapper.scrollLeft !== currentScrollPosition)
                    secondChannel.wavePlayer.drawer.wrapper.scrollLeft = currentScrollPosition;
            });

            channel.wavePlayer.on('region-in', (region) => {
                channel.currentLog.push(region.id);
            });

            channel.wavePlayer.on('region-out', (region) => {
                var index = channel.currentLog.indexOf(region.id);
                if (index > -1)
                    channel.currentLog.splice(index, 1);
            });

            channel.wavePlayer.on('region-click', (region, e) => {
                e.stopPropagation();
            });

            channel.wavePlayer.on('region-dblclick', (region, e) => {
                channel.wavePlayer.play(region.start, region.end);
                e.stopPropagation();
            });

            channel.wavePlayer.on('region-created', (region) => {
                var log = channel.logs.find(log => log.id === region.id);
                //prevent while agent channel or adding existing regions on wave creation
                if (channel.type === ChannelType.Agent || log !== undefined)
                    return;

                var nextId = (channel.logs.length ? Math.max.apply(Math, channel.logs.map(log => { return log.id; })) : 0) + 1;

                region.id = nextId;
                region.color = color.clientRegion;
                channel.logs.push(
                    {
                        id: nextId,
                        startTime: region.start,
                        startTimeFormat: moment.duration(region.start, 'seconds').format("mm:ss.SSS", { trim: false }),
                        endTime: region.end,
                        endTimeFormat: moment.duration(region.end, 'seconds').format("mm:ss.SSS", { trim: false }),
                        labels: $scope.labels.map(group => ({ id: group.id, values: [] })),
                    });
            });

            channel.wavePlayer.on('region-updated', (region) => {
                var log = channel.logs.find(log => log.id === region.id);
                //prevent while new region is creating
                if (log !== undefined) {
                    log.startTime = region.start;
                    log.startTimeFormat = moment.duration(region.start, 'seconds').format("mm:ss.SSS", { trim: false });
                    log.endTime = region.end;
                    log.endTimeFormat = moment.duration(region.end, 'seconds').format("mm:ss.SSS", { trim: false });
                }
                $scope.$digest();
            });

            channel.wavePlayer.on('audioprocess', () => {
                var currentTime = channel.wavePlayer.getCurrentTime().toFixed(3);
                channel.currentTime = currentTime;
                $scope.$digest();
            });

            channel.wavePlayer.on('error', (message) => {
                alertService.showError("ERROR_ON_FETCHING_AUDIO_FILE");
                channel.loadingFile = false;
            });
        }

        function getCurrentRegionIds(channel) {
            var currentTime = channel.wavePlayer.getCurrentTime();
            return channel.logs
                .filter(log => log.startTime <= currentTime && log.endTime >= currentTime)
                .map(log => { return log.id; });
        }

        function loadRegions(channelType) {
            var channel = getChannel(channelType);
            channel.logs.forEach((log) => {
                channel.wavePlayer.addRegion(
                    {
                        id: log.id,
                        start: log.startTime,
                        end: log.endTime,
                        color: channelType === ChannelType.Client ? color.clientRegion : (log.isBotAgent ? color.botRegion : color.agentRegion),
                        drag: true,
                        resize: channelType === ChannelType.Client
                    });
            });
        }

        hotkeys.add({
            combo: 'space',
            description: 'playPauseAudio',
            callback: (e) => {
                $scope.playPauseToggle();
                e.preventDefault();
            }
        });

        hotkeys.add({
            combo: 'enter',
            description: 'stopAudio',
            callback: (e) => {
                $scope.stopButton();
                e.preventDefault();
            }
        });

        hotkeys.add({
            combo: 'up',
            description: 'increasePlaybackRate',
            callback: (e) => {
                $scope.playbackRate += playback.step;
                $scope.setPlaybackRate();
                e.preventDefault();
            }
        });

        hotkeys.add({
            combo: 'down',
            description: 'decreasePlaybackRate',
            callback: (e) => {
                $scope.playbackRate -= playback.step;
                $scope.setPlaybackRate();
                e.preventDefault();
            }
        });

        hotkeys.add({
            combo: 'ctrl+up',
            description: 'muteClientChannel',
            callback: (e) => {
                $scope.muteChannel(ChannelType.Client);
                e.preventDefault();
            }
        });

        hotkeys.add({
            combo: 'ctrl+down',
            description: 'muteAgentChannel',
            callback: (e) => {
                $scope.muteChannel(ChannelType.Agent);
                e.preventDefault();
            }
        });

        hotkeys.add({
            combo: 'left',
            description: 'skipBackward',
            callback: (e) => {
                $scope.call.agentChannel.wavePlayer.skipBackward(seekToTime.skipStep);
                e.preventDefault();
            }
        });

        hotkeys.add({
            combo: 'right',
            description: 'skipForward',
            callback: (e) => {
                $scope.call.agentChannel.wavePlayer.skipForward(seekToTime.skipStep);
                e.preventDefault();
            }
        });

        hotkeys.add({
            combo: 'del',
            description: 'deleteCurrentRegions',
            callback: (e) => {
                if ($scope.call.clientChannel.currentLog) {
                    $scope.call.clientChannel.currentLog.forEach(logId => $scope.deleteRegion(logId));
                }
                e.preventDefault;
            }
        });

        hotkeys.add({
            combo: 'ctrl+s',
            description: 'saveState',
            callback: (e) => {
                $scope.saveTranscription();
                e.preventDefault();
            }
        });
    }
})();