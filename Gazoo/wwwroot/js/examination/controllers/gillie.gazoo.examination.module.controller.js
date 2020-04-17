(function () {
    'use strict';

    angular
        .module('Company.gazoo.examination')
        .controller('examinationModuleController', examinationModuleController);

    examinationModuleController.$inject = ['$scope', '$state'];

    function examinationModuleController($scope, $state) {
        $scope.$state = $state;

        $scope.showNavigationBar = () => {
            return $state.current.name.indexOf('examinationDetails') === -1 && $state.current.name.indexOf('reportDetails') === -1;
        };
    }
})();