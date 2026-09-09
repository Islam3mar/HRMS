using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces
{
    public interface IPayrollService
    {
        // تقرير رواتب كل الموظفين (او المفلترين باسم موظف) عن شهر/سنة معينة
        Task<PayrollReportResult> GetReportAsync(PayrollSearchFilter filter);

        // مفردات راتب موظف واحد بالتحديد (مستخدمة فى شاشة الطباعة)
        // لو فيه سجل معتمد محفوظ بالفعل بيرجعه زي ما هو، لو مفيش بيحسبه لحظيًا من غير ما يحفظه
        Task<PayrollRowDto?> GetEmployeePayrollAsync(int employeeId, int month, int year);

        // اعتماد راتب موظف عن شهر/سنة معينة: بيحسبه لحظيًا وقت الاعتماد ويحفظه Snapshot ثابت
        // لو كان معتمد بالفعل من قبل، بيرجع نفس السجل المحفوظ من غير ما يعيد حسابه او يجدده
        Task<PayrollRowDto?> ApproveAsync(int employeeId, int month, int year);

        // بيتستخدم لما يفتح شاشة التعديل: لو الراتب لسه معتمدش، بيعتمده الأول تلقائي، وبعدين يرجّع قيمه الحالية
        Task<PayrollRowDto?> GetForEditAsync(int employeeId, int month, int year);

        // حفظ التعديل اليدوي على راتب معتمد بالفعل (اجمالى الاضافى / اجمالى الخصم / الصافى)
        Task<PayrollEditResult> EditApprovedAsync(PayrollManualEditInput input);


        // اقدم سنة يسمح باختيارها (سنة تأسيس الشركة)
        int MinimumAllowedYear { get; }
    }
}
