export class TimestampSearchViewModel {

    startTimestamp: Date;

    finishTimestamp: Date;

    constructor() {
        this.finishTimestamp = null;
        this.startTimestamp = new Date();
        this.startTimestamp.setMinutes(this.startTimestamp.getMinutes() - 60);
    }
}