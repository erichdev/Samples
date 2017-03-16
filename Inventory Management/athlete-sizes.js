(function () {
    angular.module('app')
        .controller('AthleteSizesController', ['AthleteService', 'AthleteSizesService', 'Logger', 'Spinner', 'SportService',
            function (AthleteService, AthleteSizesService, Logger, Spinner, SportService) {

                var vm = this;

                vm.athleteSizes;
                vm.onSelectAthlete = onSelectAthlete;
                vm.onSelectSport = onSelectSport;
                vm.onSubmitSizeForm = onSubmitSizeForm;
                vm.selectedAthlete;
                vm.selectedSport;
                vm.sports;

                activate();

                function activate() {
                    Spinner.show();
                    getSports()
                        .finally(Spinner.hide);
                }

                function getAthleteSizes() {
                    if (vm.selectedAthlete && vm.selectedSport) {
                        return AthleteSizesService.getAthleteSize(vm.selectedAthlete.Id, vm.selectedSport)
                            .then(function (data) {
                                vm.athleteSizes = data;
                            });
                    }
                }

                function getAthletesBySport() {
                    return AthleteService.getAthletesBySportId(vm.selectedSport)
                        .then(function (data) {
                            vm.athletes = data;
                        });
                }

                function getSports() {
                    return SportService.getSports()
                        .then(function (data) {
                            vm.sports = data;
                        });
                }

                function mapAthleteSizes() {
                    return vm.athleteSizes.map(function (item) {
                        item.AthleteId = vm.selectedAthlete.Id;
                        item.SportId = vm.selectedSport;
                        return item;
                    });
                }

                function onSelectAthlete() {
                    if (vm.selectedAthlete) {
                        Spinner.show();

                        getAthleteSizes()
                            .finally(Spinner.hide);
                    }
                }

                function onSelectSport() {
                    vm.selectedAthlete = undefined;

                    Spinner.show();

                    getAthletesBySport()
                        .finally(Spinner.hide);
                }

                function onSubmitSizeForm() {
                    Spinner.show();

                    updateAthleteSizes()
                        .finally(Spinner.hide);
                }

                function updateAthleteSizes() {
                    var data = mapAthleteSizes();

                    return AthleteSizesService.updateAthleteSizes(vm.selectedAthlete.Id, vm.selectedSport, data)
                        .then(onUpdateSuccess)
                        .then(getAthleteSizes);

                    function onUpdateSuccess() {
                        Logger.success('Athlete sizes saved');
                    }
                }


            }
        ]);
})();
