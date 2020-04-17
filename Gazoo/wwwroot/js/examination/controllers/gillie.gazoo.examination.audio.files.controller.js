(function () {
    'use strict';

    angular
        .module('Company.gazoo.examination')
        .controller('audioFilesComponentController', audioFilesComponentController);

    angular
        .module('Company.gazoo.examination')
        .component('audioFilesComponent', {
            controller: 'audioFilesComponentController',
            templateUrl: '/templates/examination/audioFilesComponent.html'
        });

    audioFilesComponentController.$inject = ['$scope', '$state'];

    function audioFilesComponentController($scope, $state) {
        $scope.$state = $state;

        $scope.goToAudioFleListTab = () => {
            $state.go("audioFilesComponent.audioFileList", null, { reload: 'audioFilesComponent.audioFileList' });
        };

        $scope.goToRecordingTab = () => {
            $state.go("audioFilesComponent.audioRecording", null, { reload: 'audioFilesComponent.audioRecording' });
        };

        $scope.goToUploaderTab = () => {
            $state.go("audioFilesComponent.audioUploader", null, { reload: 'audioFilesComponent.audioUploader' });
        };

        $scope.stateHasSubstate = (substateName) => {
            return $scope.$state.current.name.indexOf(substateName) !== -1;
        };
    }
})();