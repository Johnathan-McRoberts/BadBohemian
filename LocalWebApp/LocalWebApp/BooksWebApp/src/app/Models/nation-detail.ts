export interface INationDetail {
    id: string;
    name: string;
    capital: string;
    latitude: number;
    longitude: number;
    imageUri: string;
};

export class NationDetail implements INationDetail {
    static fromData(data: INationDetail) {
        return new this(
            data.id,
            data.name,
            data.capital,
            data.latitude,
            data.longitude,
            data.imageUri);
    }

    constructor(
        public id: string = "",
        public name: string = "",
        public capital: string = "",
        public latitude: number = 0,
        public longitude: number = 0,
        public imageUri: string = "",) {
    }
}

export interface INationDetailResponse {
    updateItem: INationDetail;
    errorCode: number;
    failReason: string;
    userId: string;
};

export class NationDetailResponse implements INationDetailResponse {
    static fromData(data: INationDetailResponse) {
        var nationDetail: NationDetail;
        if (data.updateItem == undefined || data.updateItem == null)
            nationDetail = new NationDetail();
        else
            nationDetail = NationDetail.fromData(data.updateItem);

        return new this(
            nationDetail,
            data.errorCode,
            data.failReason,
            data.userId);
    }

    constructor(
        public updateItem: INationDetail = new NationDetail(),
        public errorCode: number = -1,
        public failReason: string = "",
        public userId: string = "") {
    }

}
