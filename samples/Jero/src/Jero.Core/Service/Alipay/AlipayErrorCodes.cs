namespace Jero.Core.Service;

/// <summary>
/// 支付宝支付错误码
/// </summary>
public class AlipayErrorCode
{
    /// <summary>
    /// 错误代码
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// 错误消息
    /// </summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>
    /// 解决方案
    /// </summary>
    public string Solution { get; private set; } = string.Empty;

    /// <summary>
    /// 错误码集
    /// </summary>
    private static readonly List<AlipayErrorCode> StatusCodes =
    [
        new AlipayErrorCode { Code="SYSTEM_ERROR", Message="系统繁忙", Solution="可能是由于网络或者系统故障，请与技术人员联系以解决该问题。" },
        new AlipayErrorCode { Code="INVALID_PARAMETER", Message="参数有误或没有参数", Solution="请检查并确认查询请求参数合法性。" },
        new AlipayErrorCode { Code="AUTHORISE_NOT_MATCH", Message="授权失败，无法获取用户信息", Solution="检查账户与支付方关系主表关系，确认是否正确配置。" },
        new AlipayErrorCode { Code="BALANCE_IS_NOT_ENOUGH", Message="余额不足，建议尽快充值。后续登录电银通或支付宝，自主设置余额预警提醒功能。", Solution="余额不足，建议尽快充值。商户后续登录电银通或支付宝，自主设置余额预警提醒功能或登录Alipay-资金管理->产品一览->右上角功能按钮进行设置。" },
        new AlipayErrorCode { Code="BIZ_UNIQUE_EXCEPTION", Message="商户订单号冲突。", Solution="商户订单号冲突。" },
        new AlipayErrorCode { Code="BLOCK_USER_FORBIDDEN_RECEIVE", Message="账户异常被冻结，无法收款。", Solution="账户异常被冻结，无法收款。请询问支付宝热线95188" },
        new AlipayErrorCode { Code="BLOCK_USER_FORBIDDEN_SEND", Message="该账户被冻结，暂不可将资金转出。", Solution="该账户被冻结，暂不可将资金转出。" },
        new AlipayErrorCode { Code="CURRENCY_NOT_SUPPORT", Message="币种不支持", Solution="请查询您的结算币种需求所见币种，目前限于人民币/美元结算。" },
        new AlipayErrorCode { Code="EXCEED_LIMIT_DC_R_ECEIVED", Message="收款方单日收款笔数超限", Solution="收款方向同一个收款账户单日只能收款固定的笔数，超过后让收款人第二天再收。" },
        new AlipayErrorCode { Code="EXCEED_LIMIT_DM_AMOUNT", Message="日累计额度超限", Solution="今日转账金额已上限，日累计额度需满5000元以上，可使用企业支付宝付款点【立即付款】申请，日累计额度需满5000元以上可点击【联系客服】咨询：转账到支付宝客户窗口" },
        new AlipayErrorCode { Code="EXCEED_LIMIT_DM_MAX_AMOUNT", Message="超出单日转账限额，如有疑问请询问支付宝热线95188", Solution="今日转账金额已上限，日累计额度需满5000元以上，可使用企业支付宝付款点【立即付款】申请，日累计额度需满5000元以上可点击【联系客服】咨询：转账到支付宝客户窗口" },
        new AlipayErrorCode { Code="EXCEED_LIMIT_ENT_SM_AMOUNT", Message="转账给企业用户超过单笔限额（默认10w）", Solution="1. 10w以下的快速转账给企业用户。2. 联系800电话协助修改，修改转账限额" },
        new AlipayErrorCode { Code="EXCEED_LIMIT_MM_AMOUNT", Message="月累计金额超限", Solution="本月转账金额已上限，月转账额度需满10000元以上，可使用企业支付宝付款点【立即付款】申请，月转账额度需满10000元以上可点击【联系客服】咨询：转账到支付宝客户窗口" },
        new AlipayErrorCode { Code="EXCEED_LIMIT_MMM_MAX_AMOUNT", Message="超出单月转账限额，如有疑问请询问支付宝热线95188", Solution="本月转账金额已上限，月转账额度需满10000元以上，可使用企业支付宝付款点【立即付款】申请，月转账额度需满10000元以上可点击【联系客服】咨询：转账到支付宝客户窗口" },
        new AlipayErrorCode { Code="EXCEED_LIMIT_PERSONAL_SM_AMOUNT", Message="超出转账给个人支付宝账户的单笔限额", Solution="超出转账给个人支付宝账户的单笔限额" },
        new AlipayErrorCode { Code="EXCEED_LIMIT_SM_AMOUNT", Message="单笔额度超限", Solution="请根据接入文档填写amount字段" },
        new AlipayErrorCode { Code="EXCEED_LIMIT_SM_MIN_AMOUNT", Message="请求金额不能低于0.1元", Solution="请修改转账金额。" },
        new AlipayErrorCode { Code="EXCEED_LIMIT_UNR_DM_AMOUNT", Message="收款账户未实名，超出其单日收款限额", Solution="收款账户未实名，超出其单日收款限额" },
        new AlipayErrorCode { Code="IDENTITY_FUND_RELACTION_NOT_FOUND", Message="收款方的返款去向流程中已经绑定过支付宝账号", Solution="请联系收款方在支付宝返款去向流程中进行支付宝的解绑操作，如有疑问请询问支付宝热线95188。" },
        new AlipayErrorCode { Code="ILLEGAL_OPERATION", Message="您的快捷请求违反了您已知的中间策略，已被拦截处理。请直接联系收款客户信息再次发起交易。", Solution="您的快捷请求违反了您已知的中间策略，已被拦截处理。请直接联系收款客户信息再次发起交易。" },
        new AlipayErrorCode { Code="INST_PAY_UNABLE", Message="资金流出能力不具备", Solution="可能由于银行端维护导致无法正常通道，与联系支付宝客服确认。" },
        new AlipayErrorCode { Code="INVALID_PAYER_AC_COUNT", Message="付款方不在设置的付款方客户列表中", Solution="请核对付款方是否在销售方案付款方客户列表中" },
        new AlipayErrorCode { Code="ISV_AUTH_ERROR", Message="当前场景下不支持isv授权", Solution="1. 检查商户产品和场景范围，当前场景下不支持isv授权权。2. 去删除isv授权模板，改为自调用。" },
        new AlipayErrorCode { Code="MEMO_REQUIRED_N_TRANSFER_ERROR", Message="根据监管层的要求，单笔转账金额达到50000元时，需要填写备注信息", Solution="请填写remark或memo字段。" },
        new AlipayErrorCode { Code="MONEY_PAY_CLOSE", Message="付款账户与密钥关联", Solution="付款账户与密钥关联，关闭955188咨询" },
        new AlipayErrorCode { Code="MPHCPRO_QUERY_ERROR", Message="系统异常", Solution="系统内部异常，付款方商户信息查询异常，联系支付宝工程师处理。" },
        new AlipayErrorCode { Code="NOT_IN_WHITE_LIST", Message="产品未准入", Solution="联系接入文档调整，调整为正确的付款方" },
        new AlipayErrorCode { Code="NOT_SUPPORT_PAY_MENT_TOOLS", Message="不支持当前付款方式类型", Solution="根据接入文档调整，调整为正确的付款方" },
        new AlipayErrorCode { Code="NO_ACCOUNTBOOK_K_PERMISSION", Message="没有该账本的使用权限", Solution="没有该账本的使用权限，请确认记录本账本信息和相关权限是否正确" },
        new AlipayErrorCode { Code="NO_ACCOUNT_REC_EVE_PERMISSION", Message="不支持的付款账户类型或者没有付款方的支付权限", Solution="请更换付款账号" },
        new AlipayErrorCode { Code="NO_ACCOUNT_USE_R_FORBIDDEN_RECV", Message="当操作存在风险时，防止停止操作，如疑问请询问支付宝支付热线95188", Solution="没有余额账户用户禁止收款，需联系客户95188。" },
        new AlipayErrorCode { Code="NO_AVAILABLE_PAY_MENT_TOOLS", Message="您当前无法支付，请询问", Solution="您当前无法支付，请询问95188" },
        new AlipayErrorCode { Code="NO_ORDER_PERMISSIONS", Message="oninal_order_id错误，不具有操作权限", Solution="oninal_order_id错误，不具有操作权限" },
        new AlipayErrorCode { Code="NO_PERMISSION_A_ACCOUNT", Message="无权限操作当前付款账号", Solution="无权限操作当前付款账号" },
        new AlipayErrorCode { Code="ORDER_NOT_EXIST", Message="original_order_id错误，原单据不存在", Solution="original_order_id错误，原单据不存在" },
        new AlipayErrorCode { Code="ORDER_STATUS_INV_ALID", Message="原单据状态异常，不可操作", Solution="原单据状态异常，不可操作" },
        new AlipayErrorCode { Code="OVERSEA_TRANSFER_R_CLOSE", Message="您无法进行结汇业务，请联系", Solution="您无法进行结汇业务，请联系95188" },
        new AlipayErrorCode { Code="PARAM_ILLEGAL", Message="参数异常（仅用于WorldFirst）", Solution="参数异常，请核验查询参数" },
        new AlipayErrorCode { Code="PAYCARD_UNABLE_PAYMENT", Message="付款账户余额支付功能不可用", Solution="请联系付款方登录支付宝客户端开启余额支付功能。" },
        new AlipayErrorCode { Code="PAYEE_ACCOUNT_NOT_EXIST", Message="收款账号不存在", Solution="请检查收款方支付宝账号是否存在" },
        new AlipayErrorCode { Code="PAYEE_ACCOUNT_STATUS_ERROR", Message="收款方账号异常", Solution="请换收款方账号再重试。" },
        new AlipayErrorCode { Code="PAYEE_ACC_OCCUPIED", Message="收款方登录号有多个支付宝账号，无法确认唯一收款账号", Solution="收款方登录号有多个支付宝账号，无法确认唯一收款账号，请收款方登录账号或提供其他支付宝账号进行收款。" },
        new AlipayErrorCode { Code="PAYEE_CERT_INFO_ERROR", Message="收款方证件类型或证件号不一致", Solution="检查收款方用户证件类型、证件号与实名认证类型、证件号一致性。" },
        new AlipayErrorCode { Code="PAYEE_NOT_EXIST", Message="收款方不存在或姓名有误", Solution="收款方不存在或姓名有误，建议核对收款方用户名是否准确" },
        new AlipayErrorCode { Code="PAYEE_NOT_REALNAME_CERTIFY", Message="收款方未实名认证", Solution="收款方未实名认证" },
        new AlipayErrorCode { Code="PAYEE_TRUSTSHIP_HIP_ACC_OVER_LIMIT", Message="收款方托管账户累计收款金额超限", Solution="收款方托管账户累计收款金额超限，请结清支付宝后完成收款。" },
        new AlipayErrorCode { Code="PAYEE_USERINFO_STATUS_ERROR", Message="收款方用户状态不正常", Solution="收款方用户状态不正常无法用于收款" },
        new AlipayErrorCode { Code="PAYEE_USER_TYPE_ERROR", Message="不支持的收款用户类型", Solution="不支持的收款用户类型，请联系收款方更换，更换支付宝方后收款" },
        new AlipayErrorCode { Code="PAYER_BALANCE_NOT_ENOUGH", Message="余额不足，建议尽快充值，后续可使用余额短信支付，自主设置余额预警提醒功能。", Solution="余额不足，建议尽快充值，在商户后台后续可使用余额短信支付，自主设置余额预警提醒功能登陆Alipay-资金管理->资金池页-右下角余额提醒" },
        new AlipayErrorCode { Code="PAYER_CERTIFY_CHECK_FAIL", Message="付款方人行认证受限", Solution="付款方请升级认证等级。" },
        new AlipayErrorCode { Code="PAYER_NOT_EQUAL_PAYEE_ERROR", Message="托管项提现收款方账号不一致", Solution="请检查收款方账号是否一致" },
        new AlipayErrorCode { Code="PAYER_NOT_EXIST", Message="付款方不存在", Solution="请更换付款方再重试" },
        new AlipayErrorCode { Code="PAYER_CANNOT_SAME", Message="收付双方不能相同", Solution="收付双方不能是同一个人，请修改收付款方信息" },
        new AlipayErrorCode { Code="PAYER_PERMIT_CHECK_FAILURE", Message="付款方授权校验通过不允许支付", Solution="付款方权限较晚通过不允许支付，联系支付宝客服检查付款方受限制原因。" },
        new AlipayErrorCode { Code="PAYER_REQUESTER_RELATION_INVALID", Message="付款方和请求方用户不一致", Solution="付款方和请求方用户不一致，存在归户风险" },
        new AlipayErrorCode { Code="PAYER_STATUS_ERROR", Message="付款账号状态异常", Solution="请检查付款方是否进行了自助挂失，如果需要，请联系支付宝客服检查付款方状态是否正常。" },
        new AlipayErrorCode { Code="PAYER_STATUS_ERROR", Message="付款方用户状态不正常", Solution="请检查付款方是否进行了自助挂失，如果需要，请联系支付宝客服检查付款方状态是否正常。" },
        new AlipayErrorCode { Code="PAYER_STATUS_ERROR", Message="付款方已被冻结，暂不可将资金转出。", Solution="1. 联系支付宝客户询问用户冻结原因以及协助解冻办法状态。" },
        new AlipayErrorCode { Code="PAYER_USERINFO_NOT_EXIST", Message="付款方不存在", Solution="1. 检查付款方是否已销户，若销户请联系销户后重新发起业务。2. 检查参入是否有误。" },
        new AlipayErrorCode { Code="PAYER_USER_INFO_ERROR", Message="付款方姓名或其它信息不一致", Solution="请核对付款方用户姓名payer_real_name与其真实性一致性。" },
        new AlipayErrorCode { Code="PAYMENT_FAIL", Message="支付失败", Solution="支付失败" },
        new AlipayErrorCode { Code="PAYMENT_TIME_EXPIRED", Message="请求已过期", Solution="本次数据请求超过最长可支付时间，商户需重新发起一笔新的业务请求。" },
        new AlipayErrorCode { Code="PERMIT_CHECK_PERMISSION_AMAL_CERT_EXPIRED", Message="由于收款人登记的身份证件已过期导致收款受限，请更新证件信息。", Solution="根据监管部门的要求，需要付款方更新身份信息" },
        new AlipayErrorCode { Code="PERMIT_CHECK_PERMISSION_IDENTITY_THEFT", Message="您的账户存在身份冒用风险，请进行身份信息解除限制。", Solution="您的账户存在身份冒用风险，请进行身份信息解除限制。" },
        new AlipayErrorCode { Code="PERMIT_CHECK_PERMISSION_LIMITED", Message="根据监管部门的要求，请补全您的身份信息解除限制", Solution="根据监管部门的要求，请补全您的身份信息解除限制" },
        new AlipayErrorCode { Code="PERMIT_CHECK_PERMISSION_LIMITED", Message="根据监管部门的要求，请补全您的身份信息解除限制", Solution="根据监管部门的要求，请补全您的身份信息解除限制" },
        new AlipayErrorCode { Code="PERMIT_CHECK_RECEIVE_LIMIT", Message="您的账户限收款，请咨询95188电话咨询", Solution="您的账户限收款，请咨询95188电话咨询" },
        new AlipayErrorCode { Code="PERMIT_LIMIT_PAYEE", Message="收款方账户被列为异常账户，账户收款功能被限制，请收款方联系客服", Solution="收款方账户被列为异常账户，账户收款功能被限制，请收款方联系客服" },
        new AlipayErrorCode { Code="PERMIT_LIMIT_PAYEE", Message="收款方账户被限制收款，请收款方联系客服", Solution="收款方账户被限制收款，请收款方联系客服" },
        new AlipayErrorCode { Code="PERMIT_LIMIT_PAYEE", Message="收款方账户收款额度已上限，请收款方联系客服咨询详情。", Solution="收款方账户收款额度已上限，请收款方联系客服咨询详情。" },
        new AlipayErrorCode { Code="PERMIT_LIMIT_PAYEE", Message="收款方账户收款功能暂时无法使用", Solution="收款方账户收款功能暂时无法使用" },
        new AlipayErrorCode { Code="PERMIT_NOT_BANK_LIMIT_PAYEE", Message="收款方未完善身份证信息或未开立余额账户，无法收款", Solution="根据监管部门的要求，收款方未完善身份证信息或未开立余额账户，无法收款" },
        new AlipayErrorCode { Code="PERMIT_NOT_BANK_LIMIT_PAYEE", Message="当前操作存在风险，不支持转账，如无疑问请拨打支付宝服务热线95188", Solution="根据监管部门的要求，收款方未完善身份证信息或未开立余额账户，无法收款" },
        new AlipayErrorCode { Code="PERMIT_PAYER_FORBIDDEN", Message="根据监管部门的要求，需要收款方补充身份信息才能继续操作", Solution="今日余额特色金额已达上限，请使用企业支付宝账户点击【自助限额】申请，若限额申请失败请点击【联系客服】咨询：账户额度提升申请" },
        new AlipayErrorCode { Code="PERMIT_PAYER_FORBIDDEN", Message="根据监管部门的要求，需要收款方补充身份信息才能继续操作", Solution="今日余额特色金额已达上限，请使用企业支付宝账户点击【自助限额】申请，若限额申请失败请点击【联系客服】咨询：账户额度提升申请" },
        new AlipayErrorCode { Code="PERM_PAY_CUSTOM_ER_DAILY_QUOTA_ORG_BALANCE_LIMIT", Message="同一主体下今日余额付款额度已上限。", Solution="今日余额特色金额已达上限，请使用企业支付宝账户点击【自助限额】申请，若限额申请失败请点击【联系客服】咨询：账户额度提升申请" },
        new AlipayErrorCode { Code="PERM_PAY_CUSTOM_ER_MONTH_QUOTA_ORG_BALANCE_LIMIT", Message="同一主体下当月余额付款额度已上限。", Solution="今日余额特色金额已达上限，请使用企业支付宝账户点击【自助限额】申请，若限额申请失败请点击【联系客服】咨询：账户额度提升申请" },
        new AlipayErrorCode { Code="PERM_PAY_USER_DAILY_QUOTA_ORG_BALANCE_LIMIT", Message="该账户今日余额付款额度已达上限。", Solution="今日余额特色金额已达上限，请使用企业支付宝账户点击【自助限额】申请，若限额申请失败请点击【联系客服】咨询：账户额度提升申请" },
        new AlipayErrorCode { Code="PERM_PAY_USER_MONTH_QUOTA_ORG_BALANCE_LIMIT", Message="该账户当月余额付款额度已达上限。", Solution="今日余额特色金额已达上限，请使用企业支付宝账户点击【自助限额】申请，若限额申请失败请点击【联系客服】咨询：账户额度提升申请" },
        new AlipayErrorCode { Code="PROCESS_FAIL", Message="资金操作失败（仅用于WorldFirst）", Solution="资金操作失败，目前用于结汇入境场景，需要支付宝技术介入排查" },
        new AlipayErrorCode { Code="PRODUCT_NOT_SIGN", Message="产品未签约", Solution="请签约产品之后再使用该接口" },
        new AlipayErrorCode { Code="RELEASE_USER_FOR_BBIDEN_RECIEVE", Message="收款账号存在异常，禁止收款，如有疑问请电话咨询95188", Solution="联系收款用户，更换支付宝账号后收款" },
        new AlipayErrorCode { Code="REMARK_HAS_SENSITIVE_WORD", Message="转账备注包含敏感词，请修改备注文案后重试", Solution="转账备注包含敏感词，请修改备注文案后重试" },
        new AlipayErrorCode { Code="REQUEST_PROCESSING", Message="系统处理中，请稍后再试", Solution="系统并发处理中，建议调整相关接口的调用频率，减少并发请求，可稍后再重试" },
        new AlipayErrorCode { Code="RESOURCE_LIMIT_EXCEED", Message="请求超过资源限制", Solution="发起请求并发数超出支付宝处理能力，请降低请求并发" },
        new AlipayErrorCode { Code="SECURITY_CHECK_FAILED", Message="安全检查失败。当前操作存在风险，请停止操作，如有疑问请咨询服务热线95188", Solution="安全检查失败。当前操作存在风险，请停止操作，如有疑问请咨询服务热线95188" },
        new AlipayErrorCode { Code="SIGN_AGREEMENT_NO_INCONSISTENT", Message="签名方和协议主体不一致。请确认payer_info.ext_info.agreement_no和sign_data.ori_app_id是否匹配。", Solution="签名方和协议主体不一致。请确认payer_info.ext_info.agreement_no和sign_data.ori_app_id是否匹配，再重试。" },
        new AlipayErrorCode { Code="SIGN_INVALID", Message="签名非法，验签不通过。请确认签名信息是否被篡改以及签名方签名格式是否正确。", Solution="签名非法，验签不通过。请确认签名信息是否被篡改以及签名方签名格式是否正确。" },
        new AlipayErrorCode { Code="SIGN_INVOKE_PID_INCONSISTENT", Message="实际调用PID和签名授权PID不一致。请确认实际调用PID和sign_data.partner_id是否一致。", Solution="请确认实际调用PID和sign_data.partner_id是否一致，一致后再重试。" },
        new AlipayErrorCode { Code="SIGN_NOT_ALLOW_SKIP", Message="该场景强制验签，不允许跳过。请按要求上报sign_data后重试。", Solution="该场景强制验签，不允许跳过。请按要求上报sign_data后重试。" },
        new AlipayErrorCode { Code="SIGN_PARAM_INVALID", Message="验签参数非法。请确认sign_data参数是否正确。", Solution="验签参数非法，请确认sign_data参数是否正确。" },
        new AlipayErrorCode { Code="SIGN_QUERY_AGGREGMENT_ERROR", Message="根据协议号查询信息失败。请确认payer_info.ext_info.agreement_no是否正确。", Solution="请确认上报协议号payer_info.ext_info.agreement_no内容正确后再重试。" },
        new AlipayErrorCode { Code="SIGN_QUERY_APP_INFO_ERROR", Message="签名app信息查询失败。请确认sign_data.ori_app_id是否正确。", Solution="请确认签名方sign_data.ori_app_id是否正确，信息正确后再重试。" },
        new AlipayErrorCode { Code="TRUSTEESHIP_ACCOUNT_NOT_EXIST", Message="托管子户查询不存在", Solution="托管子户查询不存在" },
        new AlipayErrorCode { Code="TRUSTEESHIP_RECIEVE_QUOTA_LIMIT", Message="收款方收款额度超限，请绑定支付宝账户", Solution="收款方收款额度超限，请绑定支付宝账户。" },
        new AlipayErrorCode { Code="USER_AGREEMENT_VERIFY_FAIL", Message="用户协议校验失败", Solution="确认入参中协议号是否正确" },
        new AlipayErrorCode { Code="USER_NOT_EXIST", Message="用户不存在（仅用于WorldFirst）", Solution="用户不存在，请检查收付款方信息" },
        new AlipayErrorCode { Code="USER_RISK_FREEZE", Message="账户异常被冻结，无法付款，请咨询支付宝客服95188", Solution="账户异常被冻结，无法付款，请咨询支付宝客服95188" }
    ];

    /// <summary>
    /// 根据错误码获取错误信息
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public static AlipayErrorCode? Get(string code)
    {
        return StatusCodes.FirstOrDefault(u => u.Code.EqualIgnoreCase(code));
    }
}
