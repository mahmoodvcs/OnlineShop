insert into auth.users ("id", "first_name", "last_name", "phone_number", "national_code", "created_at")
select id, first_name, last_name, mobile_number as phone_number, national_code, now()
from mahta.users