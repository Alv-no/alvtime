use chrono::{DateTime, Duration, Local, Timelike};

pub fn round_floor_quarter(dt: DateTime<Local>) -> DateTime<Local> {
    let minute = dt.minute();
    let new_minute = (minute / 15) * 15;
    dt.with_minute(new_minute).unwrap()
        .with_second(0).unwrap()
        .with_nanosecond(0).unwrap()
}

pub fn round_ceil_quarter(dt: DateTime<Local>) -> DateTime<Local> {
    let minute = dt.minute();
    let remainder = minute % 15;
    if remainder == 0 && dt.second() == 0 && dt.nanosecond() == 0 {
        return dt;
    }
    let floor = round_floor_quarter(dt);
    floor + Duration::minutes(15)
}